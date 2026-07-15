using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using FortniteSwapper.Models;

namespace FortniteSwapper.Services;

public class SwapResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public interface ISwapService
{
    SwapResult ApplySwap(string sourceId, string targetId, string sourceName, string targetName, string version, string installPath);
    SwapResult RestoreSwap(SwapRecord record, string installPath);
    SwapResult RestoreAll(string installPath);
    List<SwapRecord> GetAppliedSwaps();
}

public class SwapService : ISwapService
{
    private readonly IMappingService _mappings;
    private readonly IFortniteLocator _locator;
    private readonly IBackupService _backup;
    private readonly ILogService _log;
    private readonly string _manifestPath;

    public SwapService(IMappingService mappings, IFortniteLocator locator, IBackupService backup, ILogService log, string? manifestPath = null)
    {
        _mappings = mappings;
        _locator = locator;
        _backup = backup;
        _log = log;
        _manifestPath = manifestPath ?? Paths.SwapManifestFile;
    }

    public SwapResult ApplySwap(string sourceId, string targetId, string sourceName, string targetName, string version, string installPath)
    {
        try
        {
            if (_locator.IsGameRunning())
            {
                return Fail("Fortnite is running. Close the game completely before swapping.");
            }

            if (!_locator.IsValidInstall(installPath))
            {
                return Fail("Fortnite install path is invalid. Set it in Settings.");
            }

            if (!_mappings.HasEntry(sourceId) || !_mappings.HasEntry(targetId))
            {
                return Fail("No mapping entry for the selected cosmetic(s). Update mappings in Settings for this version.");
            }

            var paks = _locator.PaksDirectory(installPath);
            var src = _mappings.GetEntry(sourceId)!;
            var tgt = _mappings.GetEntry(targetId)!;

            var targetPak = Path.Combine(paks, tgt.Pak);
            var targetSig = Path.Combine(paks, tgt.Sig);

            if (!File.Exists(targetPak))
            {
                return Fail($"Target pak not found on disk: {tgt.Pak}. The mapping may not match this install.");
            }

            _log.Info($"Backing up original '{src.Pak}' before swap...");
            var backupPath = _backup.BackupPakAndSig(paks, src.Pak, src.Sig);

            _log.Info($"Mirroring target '{tgt.Pak}' over source '{src.Pak}'...");
            MirrorFile(targetPak, Path.Combine(paks, src.Pak));
            if (File.Exists(targetSig))
            {
                MirrorFile(targetSig, Path.Combine(paks, src.Sig));
            }
            else if (File.Exists(Path.Combine(paks, src.Sig)))
            {
                File.Delete(Path.Combine(paks, src.Sig));
            }

            var record = new SwapRecord
            {
                SourceId = sourceId,
                TargetId = targetId,
                SourceName = sourceName,
                TargetName = targetName,
                Version = version,
                AppliedAt = DateTime.Now,
                BackupPath = backupPath,
                SourcePak = src.Pak,
                SourceSig = src.Sig
            };
            AppendRecord(record);

            _log.Info("Swap applied successfully.");
            return new SwapResult
            {
                Success = true,
                Message = "Swap applied. Launch Fortnite to see the change on your screen only — other players still see your real cosmetics."
            };
        }
        catch (Exception ex)
        {
            _log.Error("Swap failed", ex);
            return new SwapResult { Success = false, Message = $"Swap failed: {ex.Message}" };
        }
    }

    public SwapResult RestoreSwap(SwapRecord record, string installPath)
    {
        try
        {
            if (_locator.IsGameRunning())
            {
                return Fail("Fortnite is running. Close the game completely before restoring.");
            }

            if (!_locator.IsValidInstall(installPath))
            {
                return Fail("Fortnite install path is invalid. Set it in Settings.");
            }

            var paks = _locator.PaksDirectory(installPath);
            _log.Info($"Restoring original '{record.SourcePak}' from backup...");
            _backup.Restore(record.BackupPath, paks, record.SourcePak, record.SourceSig);

            var list = ReadManifest();
            list.RemoveAll(x => x.BackupPath == record.BackupPath);
            WriteManifest(list);

            _log.Info("Restore complete.");
            return new SwapResult { Success = true, Message = "Cosmetic restored to its original file." };
        }
        catch (Exception ex)
        {
            _log.Error("Restore failed", ex);
            return new SwapResult { Success = false, Message = $"Restore failed: {ex.Message}" };
        }
    }

    public SwapResult RestoreAll(string installPath)
    {
        var records = ReadManifest();
        if (records.Count == 0)
        {
            return new SwapResult { Success = true, Message = "Nothing to restore — no swaps have been applied." };
        }

        int restored = 0;
        foreach (var record in records)
        {
            var r = RestoreSwap(record, installPath);
            if (r.Success) restored++;
        }

        return new SwapResult
        {
            Success = true,
            Message = $"Restored {restored} of {records.Count} swap(s) to original files."
        };
    }

    public List<SwapRecord> GetAppliedSwaps() => ReadManifest();

    private static void MirrorFile(string from, string to)
    {
        var tmp = to + ".swap.tmp";
        File.Copy(from, tmp, true);
        if (File.Exists(to)) File.Delete(to);
        File.Move(tmp, to);
    }

    private List<SwapRecord> ReadManifest()
    {
        try
        {
            if (!File.Exists(_manifestPath)) return new List<SwapRecord>();
            var list = JsonSerializer.Deserialize<List<SwapRecord>>(File.ReadAllText(_manifestPath));
            return list ?? new List<SwapRecord>();
        }
        catch
        {
            return new List<SwapRecord>();
        }
    }

    private void WriteManifest(List<SwapRecord> records)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_manifestPath)!);
        File.WriteAllText(_manifestPath, JsonSerializer.Serialize(records, new JsonSerializerOptions { WriteIndented = true }));
    }

    private void AppendRecord(SwapRecord record)
    {
        var list = ReadManifest();
        list.Add(record);
        WriteManifest(list);
    }

    private SwapResult Fail(string message)
    {
        _log.Warn(message);
        return new SwapResult { Success = false, Message = message };
    }
}
