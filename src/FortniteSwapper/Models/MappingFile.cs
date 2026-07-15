using System.Collections.Generic;

namespace FortniteSwapper.Models;

/// <summary>
/// A version-keyed mapping of cosmetic id -> pak/sig files. This data is
/// per-Fortnite-patch and MUST be supplied by the user/community; the app ships
/// only an illustrative sample. The app refuses to swap when no mapping exists
/// for the detected version.
/// </summary>
public class MappingFile
{
    public string Version { get; set; } = string.Empty;
    public Dictionary<string, MappingEntry> Cosmetics { get; set; } = new();
}
