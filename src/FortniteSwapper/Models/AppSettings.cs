using System.Collections.Generic;

namespace FortniteSwapper.Models;

/// <summary>Persisted user settings (stored as JSON in LocalApplicationData).</summary>
public class AppSettings
{
    /// <summary>Path to the Fortnite install root (the folder containing FortniteGame).</summary>
    public string InstallPath { get; set; } = string.Empty;

    /// <summary>Selected/auto-detected Fortnite version used to choose the mapping.</summary>
    public string SelectedVersion { get; set; } = string.Empty;

    /// <summary>UI theme: "Dark" or "Light".</summary>
    public string Theme { get; set; } = "Dark";

    /// <summary>Cosmetic ids the user has marked as owned ("My Locker").</summary>
    public List<string> MyLocker { get; set; } = new();

    /// <summary>Folder where log files are written.</summary>
    public string LogsPath { get; set; } = string.Empty;

    /// <summary>Whether the user has acknowledged the ToS / anti-cheat disclaimer in Settings.</summary>
    public bool DisclaimerAccepted { get; set; }
}
