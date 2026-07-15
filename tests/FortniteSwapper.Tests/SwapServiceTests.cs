using System;
using System.IO;
using FortniteSwapper.Models;
using FortniteSwapper.Services;
using Xunit;

namespace FortniteSwapper.Tests;

public class MappingServiceTests
{
    [Fact]
    public void Import_Then_Lookup_Works()
    {
        var root = Path.Combine(Path.GetTempPath(), "fs_test_maps_" + Guid.NewGuid().ToString("N"));
        try
        {
            var svc = new MappingService(root);
            svc.Import("{\"version\":\"1.0\",\"cosmetics\":{\"A\":{\"pak\":\"a.pak\",\"sig\":\"a.sig\"}}}");

            Assert.Equal("1.0", svc.LoadedVersion);
            Assert.True(svc.HasEntry("A"));
            Assert.Equal("a.pak", svc.GetEntry("A")!.Pak);
            Assert.Equal(1, svc.Coverage);

            // Round-trips through the versioned file on disk.
            var reloaded = new MappingService(root);
            Assert.True(reloaded.LoadForVersion("1.0"));
            Assert.True(reloaded.HasEntry("A"));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void LoadForVersion_MissingVersion_ReturnsFalse()
    {
        var root = Path.Combine(Path.GetTempPath(), "fs_test_maps_" + Guid.NewGuid().ToString("N"));
        try
        {
            var svc = new MappingService(root);
            Assert.False(svc.LoadForVersion("does-not-exist"));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }
}

public class BackupServiceTests
{
    [Fact]
    public void Backup_Then_Restore_RestoresOriginal()
    {
        var baseDir = Path.Combine(Path.GetTempPath(), "fs_test_bk_" + Guid.NewGuid().ToString("N"));
        var paks = Path.Combine(baseDir, "Paks");
        var backups = Path.Combine(baseDir, "Backups");
        Directory.CreateDirectory(paks);
        File.WriteAllText(Path.Combine(paks, "a.pak"), "ORIGINAL");
        File.WriteAllText(Path.Combine(paks, "a.sig"), "ORIGINAL-SIG");

        try
        {
            var svc = new BackupService(backups);
            var backupPath = svc.BackupPakAndSig(paks, "a.pak", "a.sig");

            // Simulate an edit.
            File.WriteAllText(Path.Combine(paks, "a.pak"), "CHANGED");

            svc.Restore(backupPath, paks, "a.pak", "a.sig");

            Assert.Equal("ORIGINAL", File.ReadAllText(Path.Combine(paks, "a.pak")));
        }
        finally
        {
            if (Directory.Exists(baseDir)) Directory.Delete(baseDir, true);
        }
    }
}

public class SwapServiceTests
{
    [Fact]
    public void ApplySwap_MirrorsTarget_And_RestoreReverses()
    {
        var baseDir = Path.Combine(Path.GetTempPath(), "fs_test_swap_" + Guid.NewGuid().ToString("N"));
        var install = Path.Combine(baseDir, "Fortnite");
        var paks = Path.Combine(install, "FortniteGame", "Content", "Paks");
        var backups = Path.Combine(baseDir, "Backups");
        var manifest = Path.Combine(baseDir, "appliedSwaps.json");
        Directory.CreateDirectory(paks);

        // source = what the user owns; target = the look to show
        File.WriteAllText(Path.Combine(paks, "src.pak"), "SOURCE");
        File.WriteAllText(Path.Combine(paks, "src.sig"), "SOURCE-SIG");
        File.WriteAllText(Path.Combine(paks, "tgt.pak"), "TARGET");
        File.WriteAllText(Path.Combine(paks, "tgt.sig"), "TARGET-SIG");

        try
        {
            var mappings = new MappingService(Path.Combine(baseDir, "maps"));
            mappings.Import("{\"version\":\"1.0\",\"cosmetics\":{" +
                            "\"SRC\":{\"pak\":\"src.pak\",\"sig\":\"src.sig\"}," +
                            "\"TGT\":{\"pak\":\"tgt.pak\",\"sig\":\"tgt.sig\"}}}");

            var locator = new FortniteLocator();
            var backup = new BackupService(backups);
            var log = new LogService(Path.Combine(baseDir, "logs"));
            var swap = new SwapService(mappings, locator, backup, log, manifest);

            var result = swap.ApplySwap("SRC", "TGT", "Source", "Target", "1.0", install);
            Assert.True(result.Success, result.Message);

            // source file now mirrors the target content
            Assert.Equal("TARGET", File.ReadAllText(Path.Combine(paks, "src.pak")));

            var applied = swap.GetAppliedSwaps();
            Assert.Single(applied);

            var restore = swap.RestoreSwap(applied[0], install);
            Assert.True(restore.Success, restore.Message);
            Assert.Equal("SOURCE", File.ReadAllText(Path.Combine(paks, "src.pak")));
        }
        finally
        {
            if (Directory.Exists(baseDir)) Directory.Delete(baseDir, true);
        }
    }

    [Fact]
    public void ApplySwap_WithoutMapping_Fails()
    {
        var baseDir = Path.Combine(Path.GetTempPath(), "fs_test_swap2_" + Guid.NewGuid().ToString("N"));
        var install = Path.Combine(baseDir, "Fortnite");
        var paks = Path.Combine(install, "FortniteGame", "Content", "Paks");
        Directory.CreateDirectory(paks);
        File.WriteAllText(Path.Combine(paks, "src.pak"), "SOURCE");

        try
        {
            var mappings = new MappingService(Path.Combine(baseDir, "maps"));
            // No mapping imported.
            var locator = new FortniteLocator();
            var backup = new BackupService(Path.Combine(baseDir, "Backups"));
            var log = new LogService(Path.Combine(baseDir, "logs"));
            var swap = new SwapService(mappings, locator, backup, log, Path.Combine(baseDir, "m.json"));

            var result = swap.ApplySwap("SRC", "TGT", "Source", "Target", "1.0", install);
            Assert.False(result.Success);
        }
        finally
        {
            if (Directory.Exists(baseDir)) Directory.Delete(baseDir, true);
        }
    }
}
