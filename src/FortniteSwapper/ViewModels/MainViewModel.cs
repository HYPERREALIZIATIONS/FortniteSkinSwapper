using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace FortniteSwapper.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly INavigationService _nav;

    public HomeViewModel Home { get; }
    public BrowseViewModel Browse { get; }
    public SwapViewModel Swap { get; }
    public SettingsViewModel Settings { get; }

    [ObservableProperty]
    private ViewModelBase _current = null!;

    public override string Title => "Fortnite Swapper";

    public MainViewModel(HomeViewModel home, BrowseViewModel browse, SwapViewModel swap, SettingsViewModel settings, INavigationService nav)
    {
        Home = home;
        Browse = browse;
        Swap = swap;
        Settings = settings;
        _nav = nav;
        _nav.NavigationRequested += v => Navigate(v);
        _current = Home;
        Home.Refresh();
    }

    [RelayCommand]
    public void Navigate(string target)
    {
        Current = target switch
        {
            "Browse" => Browse,
            "Swap" => Swap,
            "Settings" => Settings,
            _ => Home
        };

        if (Current == Browse) _ = Browse.LoadAsync();
        if (Current == Home) Home.Refresh();
        if (Current == Swap) Swap.OnNavigatedTo();
    }
}
