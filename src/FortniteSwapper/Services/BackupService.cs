using System.IO;
using FortniteSwapper.Models;

namespace FortniteSwapper.Services;

public interface IBackupService
{
    /// <summary>Copies the source pak + sig into a timestamped backup folder. Returns the backup folder path.</summary>
    string BackupPakAndSig(string paksDir, string sourcePak, string sourceSig);

    /// <summary>Restores the source pak + sig from a previous backup folder.</summary>
    void Restore(string backupPath, string paksDir, string sourcePak, string sourceSig);
}

public class BackupService : IBackupService
{
    private readonly string _backupsRoot;

    public BackupService(string? backupsRoot = null)
    {
        _backupsRoot = backupsRoot ?? Paths.BackupsDir;
        Directory.CreateDirectory(_backupsRoot);
    }

    public string BackupPakAndSig(string paksDir, string sourcePak, string sourceSig)
    {
        var backupDir = Path.Combine(_backupsRoot, System.DateTime.Now.ToString("yyyyMMdd_HHmmss_fff"));
        Directory.CreateDirectory(backupDir);

        var pakSrc = Path.Combine(paksDir, sourcePak);
        var sigSrc = Path.Combine(paksDir, sourceSig);

        if (File.Exists(pakSrc)) File.Copy(pakSrc, Path.Combine(backupDir, sourcePak), true);
        if (File.Exists(sigSrc)) File.Copy(sigSrc, Path.Combine(backupDir, sourceSig), true);

        return backupDir;
    }

    public void Restore(string backupPath, string paksDir, string sourcePak, string sourceSig)
    {
        var pakBak = Path.Combine(backupPath, sourcePak);
        var sigBak = Path.Combine(backupPath, sourceSig);

        if (File.Exists(pakBak)) File.Copy(pakBak, Path.Combine(paksDir, sourcePak), true);
        if (File.Exists(sigBak)) File.Copy(sigBak, Path.Combine(paksDir, sourceSig), true);
    }
}
