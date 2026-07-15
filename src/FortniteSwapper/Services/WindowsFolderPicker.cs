using System.Windows.Forms;

namespace FortniteSwapper.Services;

/// <summary>Windows implementation of <see cref="IFolderPicker"/> using the WinForms folder browser.</summary>
public class WindowsFolderPicker : IFolderPicker
{
    public string? PickFolder(string? initialPath = null)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Select your Fortnite install folder (the one containing 'FortniteGame').",
            UseDescriptionForTitle = true
        };

        if (!string.IsNullOrWhiteSpace(initialPath) && Directory.Exists(initialPath))
        {
            dialog.InitialDirectory = initialPath;
        }

        return dialog.ShowDialog() == DialogResult.OK ? dialog.SelectedPath : null;
    }
}
