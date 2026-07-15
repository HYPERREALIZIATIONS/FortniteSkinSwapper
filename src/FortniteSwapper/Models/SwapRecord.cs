using System;

namespace FortniteSwapper.Models;

/// <summary>
/// A record of an applied swap, written to the swap manifest so it can be
/// restored later. The original files are copied to <see cref="BackupPath"/>.
/// </summary>
public class SwapRecord
{
    public string SourceId { get; set; } = string.Empty;
    public string TargetId { get; set; } = string.Empty;
    public string SourceName { get; set; } = string.Empty;
    public string TargetName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public DateTime AppliedAt { get; set; }
    public string BackupPath { get; set; } = string.Empty;
    public string SourcePak { get; set; } = string.Empty;
    public string SourceSig { get; set; } = string.Empty;
}
