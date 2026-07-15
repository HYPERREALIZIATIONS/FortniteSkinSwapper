namespace FortniteSwapper.Services;

/// <summary>Abstraction over a folder-picker dialog so the ViewModels stay UI-framework agnostic.</summary>
public interface IFolderPicker
{
    /// <summary>Shows a folder picker. Returns the selected path, or null if cancelled.</summary>
    string? PickFolder(string? initialPath = null);
}
