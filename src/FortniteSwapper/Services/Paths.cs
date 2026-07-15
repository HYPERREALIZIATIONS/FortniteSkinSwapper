using System;
using System.IO;

namespace FortniteSwapper.Services;

/// <summary>
/// Centralised, version-stable locations for all local app data (settings, cache,
/// logs, backups, swap manifest, and imported mappings).
/// </summary>
public static class Paths
{
    public static string AppDataDir =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FortniteSwapper");

    public static string LogsDir => Path.Combine(AppDataDir, "logs");
    public static string BackupsDir => Path.Combine(AppDataDir, "Backups");
    public static string SettingsFile => Path.Combine(AppDataDir, "settings.json");
    public static string CacheFile => Path.Combine(AppDataDir, "cosmetics.cache.json");
    public static string SwapManifestFile => Path.Combine(AppDataDir, "appliedSwaps.json");
    public static string MappingsDir => Path.Combine(AppDataDir, "mappings");
}
