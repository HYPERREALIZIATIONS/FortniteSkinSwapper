using System.Diagnostics;
using System.IO;

namespace FortniteSwapper.Services;

public interface IFortniteLocator
{
    bool TryDetect(out string installPath);
    bool IsValidInstall(string path);
    string PaksDirectory(string installPath);
    string? DetectVersion(string installPath);
    bool IsGameRunning();
}

public class FortniteLocator : IFortniteLocator
{
    public bool TryDetect(out string installPath)
    {
        installPath = @"C:\Program Files\Epic Games\Fortnite";
        if (IsValidInstall(installPath)) return true;

        installPath = @"C:\Program Files (x86)\Epic Games\Fortnite";
        if (IsValidInstall(installPath)) return true;

        installPath = string.Empty;
        return false;
    }

    public bool IsValidInstall(string path) =>
        !string.IsNullOrWhiteSpace(path) &&
        Directory.Exists(Path.Combine(path, "FortniteGame", "Content", "Paks"));

    public string PaksDirectory(string installPath) =>
        Path.Combine(installPath, "FortniteGame", "Content", "Paks");

    public string? DetectVersion(string installPath)
    {
        // Best effort: read the product version from the shipping executable.
        var exe = Path.Combine(installPath, "FortniteGame", "Binaries", "Win64", "FortniteClient-Win64-Shipping.exe");
        if (File.Exists(exe))
        {
            try
            {
                var vi = FileVersionInfo.GetVersionInfo(exe);
                if (!string.IsNullOrWhiteSpace(vi.ProductVersion))
                {
                    return vi.ProductVersion;
                }
            }
            catch
            {
                // fall through to pak-name parsing
            }
        }

        // Fallback: parse a version-like suffix from pak file names,
        // e.g. FortniteClient-Windows-34.10.pak -> "34.10"
        try
        {
            foreach (var f in Directory.EnumerateFiles(PaksDirectory(installPath), "*.pak"))
            {
                var name = Path.GetFileNameWithoutExtension(f);
                var idx = name.LastIndexOf('-');
                if (idx > 0)
                {
                    var candidate = name.Substring(idx + 1);
                    if (System.Text.RegularExpressions.Regex.IsMatch(candidate, @"^\d+\.\d+"))
                    {
                        return candidate;
                    }
                }
            }
        }
        catch
        {
            // ignore
        }

        return null;
    }

    public bool IsGameRunning()
    {
        foreach (var p in Process.GetProcesses())
        {
            try
            {
                if (p.ProcessName.IndexOf("Fortnite", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }
            catch
            {
                // Accessing some processes throws; ignore.
            }
        }

        return false;
    }
}
