using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FortniteSwapper.Models;
using FortniteSwapper.Services;

namespace FortniteSwapper.ViewModels;

public partial class HomeViewModel : ViewModelBase
{
    private readonly ISettingsService _settings;
    private readonly IFortniteLocator _locator;
    private readonly IMappingService _mappings;
    private readonly ISwapService _swap;
    private readonly INavigationService _nav;

    [ObservableProperty] private string _status = "Checking...";
    [ObservableProperty] private string _fortnitePath = string.Empty;
    [ObservableProperty] private string _version = string.Empty;
    [ObservableProperty] private int _mappingCoverage;
    [ObservableProperty] private string _restoreMessage = string.Empty;

    public override string Title => "Home";

    public HomeViewModel(ISettingsService settings, IFortniteLocator locator, IMappingService mappings, ISwapService swap, INavigationService nav)
    {
        _settings = settings;
        _locator = locator;
        _mappings = mappings;
        _swap = swap;
        _nav = nav;
    }

    public void Refresh()
    {
        var path = _settings.Current.InstallPath;
        FortnitePath = string.IsNullOrWhiteSpace(path) ? "(not set)" : path;

        if (_locator.IsValidInstall(path))
        {
            Version = _locator.DetectVersion(path) ?? _settings.Current.SelectedVersion;
            Status = "Fortnite install found.";
        }
        else
        {
            Version = _settings.Current.SelectedVersion;
            Status = "Fortnite not found. Set the install path in Settings.";
        }

        MappingCoverage = _mappings.LoadForVersion(Version) ? _mappings.Coverage : 0;
        RestoreMessage = string.Empty;
    }

    [RelayCommand]
    private void GoToBrowse() => _nav.RequestNavigate("Browse");

    [RelayCommand]
    private void GoToSwap() => _nav.RequestNavigate("Swap");

    [RelayCommand]
    private void GoToSettings() => _nav.RequestNavigate("Settings");

    [RelayCommand]
    private void RestoreAll()
    {
        var result = _swap.RestoreAll(_settings.Current.InstallPath);
        RestoreMessage = result.Message;
        Refresh();
    }
}
