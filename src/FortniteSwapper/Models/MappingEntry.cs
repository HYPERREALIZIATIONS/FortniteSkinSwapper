namespace FortniteSwapper.Models;

/// <summary>
/// Maps a single cosmetic id to the local .pak / .sig files that represent it
/// inside FortniteGame\Content\Paks for a given game version.
/// </summary>
public class MappingEntry
{
    public string Pak { get; set; } = string.Empty;
    public string Sig { get; set; } = string.Empty;
}
