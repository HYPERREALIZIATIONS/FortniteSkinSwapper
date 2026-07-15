using System.IO;
using System.Text.Json;
using FortniteSwapper.Models;

namespace FortniteSwapper.Services;

public interface ISettingsService
{
    AppSettings Current { get; }
    void Load();
    void Save();
}

public class SettingsService : ISettingsService
{
    private readonly string _file;
    private readonly string _logsDir;

    public AppSettings Current { get; private set; } = new();

    public SettingsService(string? settingsFile = null, string? logsDir = null)
    {
        _file = settingsFile ?? Paths.SettingsFile;
        _logsDir = logsDir ?? Paths.LogsDir;
        Load();
    }

    public void Load()
    {
        try
        {
            if (File.Exists(_file))
            {
                var json = File.ReadAllText(_file);
                var loaded = JsonSerializer.Deserialize<AppSettings>(json);
                if (loaded is not null) Current = loaded;
            }
        }
        catch
        {
            Current = new AppSettings();
        }

        if (string.IsNullOrEmpty(Current.LogsPath))
        {
            Current.LogsPath = _logsDir;
        }
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_file)!);
            File.WriteAllText(_file, JsonSerializer.Serialize(Current, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch
        {
            // Best-effort; settings are non-critical.
        }
    }
}
