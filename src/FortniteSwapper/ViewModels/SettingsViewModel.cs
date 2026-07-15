using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FortniteSwapper.Models;
using FortniteSwapper.Services;
using System.Diagnostics;

namespace FortniteSwapper.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly ISettingsService _settings;
    private readonly IFortniteLocator _locator;
    private readonly IMappingService _mappings;
    private readonly ILogService _log;
    private readonly IFolderPicker _picker;

    [ObservableProperty] private string _installPath = string.Empty;
    [ObservableProperty] private string _selectedVersion = string.Empty;
    [ObservableProperty] private string _theme = "Dark";
    [ObservableProperty] private string _logsPath = string.Empty;
    [ObservableProperty] private int _mappingCoverage;
    [ObservableProperty] private string _mappingEditorJson = string.Empty;
    [ObservableProperty] private string _mappingMessage = string.Empty;
    [ObservableProperty] private bool _disclaimerAccepted;

    public List<string> Themes { get; } = new() { "Dark", "Light" };

    public override string Title => "Settings";

    public SettingsViewModel(ISettingsService settings, IFortniteLocator locator, IMappingService mappings, ILogService log, IFolderPicker picker)
    {
        _settings = settings;
        _locator = locator;
        _mappings = mappings;
        _log = log;
        _picker = picker;

        InstallPath = _settings.Current.InstallPath;
        SelectedVersion = _settings.Current.SelectedVersion;
        Theme = _settings.Current.Theme;
        LogsPath = _settings.Current.LogsPath;
        DisclaimerAccepted = _settings.Current.DisclaimerAccepted;

        RefreshMappings();
    }

    [RelayCommand]
    private void BrowsePath()
    {
        var picked = _picker.PickFolder(InstallPath);
        if (picked is null) return;

        InstallPath = picked;
        if (_locator.IsValidInstall(InstallPath))
        {
            var v = _locator.DetectVersion(InstallPath);
            if (!string.IsNullOrWhiteSpace(v)) SelectedVersion = v;
            MappingMessage = $"Fortnite install validated. Detected version: {SelectedVersion}.";
        }
        else
        {
            MappingMessage = "That folder does not look like a Fortnite install (missing FortniteGame\\Content\\Paks).";
        }
    }

    [RelayCommand]
    private void AutoDetect()
    {
        if (_locator.TryDetect(out var path))
        {
            InstallPath = path;
            var v = _locator.DetectVersion(path);
            if (!string.IsNullOrWhiteSpace(v)) SelectedVersion = v;
            MappingMessage = $"Detected Fortnite at {path} (version {SelectedVersion}).";
        }
        else
        {
            MappingMessage = "Could not auto-detect Fortnite. Set the path manually.";
        }
    }

    [RelayCommand]
    private void Save()
    {
        _settings.Current.InstallPath = InstallPath;
        _settings.Current.SelectedVersion = SelectedVersion;
        _settings.Current.Theme = Theme;
        _settings.Current.LogsPath = LogsPath;
        _settings.Current.DisclaimerAccepted = DisclaimerAccepted;
        _settings.Save();

        RefreshMappings();
        MappingMessage = "Settings saved.";
    }

    [RelayCommand]
    private void ImportMapping()
    {
        if (string.IsNullOrWhiteSpace(MappingEditorJson))
        {
            MappingMessage = "Paste mapping JSON into the box first.";
            return;
        }

        try
        {
            _mappings.Import(MappingEditorJson);
            SelectedVersion = _mappings.LoadedVersion;
            RefreshMappings();
            MappingMessage = $"Imported mapping for version '{_mappings.LoadedVersion}' ({_mappings.Coverage} entries).";
        }
        catch (System.Exception ex)
        {
            MappingMessage = $"Import failed: {ex.Message}";
        }
    }

    [RelayCommand]
    private void OpenLogs()
    {
        try
        {
            var dir = string.IsNullOrWhiteSpace(LogsPath) ? Paths.LogsDir : LogsPath;
            Process.Start("explorer.exe", dir);
        }
        catch (System.Exception ex)
        {
            MappingMessage = $"Could not open logs: {ex.Message}";
        }
    }

    private void RefreshMappings()
    {
        MappingCoverage = _mappings.LoadForVersion(SelectedVersion) ? _mappings.Coverage : 0;
    }
}
