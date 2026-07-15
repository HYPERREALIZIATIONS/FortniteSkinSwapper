using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FortniteSwapper.Models;
using FortniteSwapper.Services;
using System.Collections.ObjectModel;

namespace FortniteSwapper.ViewModels;

public partial class BrowseViewModel : ViewModelBase
{
    private readonly ICatalogService _catalog;
    private readonly ISettingsService _settings;
    private readonly INavigationService _nav;

    public ObservableCollection<string> OwnedIds { get; } = new();

    [ObservableProperty] private ObservableCollection<Cosmetic> _filtered = new();
    [ObservableProperty] private string _search = string.Empty;
    [ObservableProperty] private string _typeFilter = "All";
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string _statusMessage = "Loading cosmetics...";
    [ObservableProperty] private int _ownedCount;

    public List<string> TypeFilters { get; } = new()
    {
        "All", "Outfit", "Backpack", "Pickaxe", "Emote", "Glider", "Wrap", "Other"
    };

    public override string Title => "Browse Cosmetics";

    public BrowseViewModel(ICatalogService catalog, ISettingsService settings, INavigationService nav)
    {
        _catalog = catalog;
        _settings = settings;
        _nav = nav;
        _catalog.StatusChanged += msg => StatusMessage = msg;
        _catalog.Items.CollectionChanged += (_, _) => ApplyFilter();
        foreach (var id in _settings.Current.MyLocker) OwnedIds.Add(id);
        OwnedCount = _settings.Current.MyLocker.Count;
        ApplyFilter();
    }

    public async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            await _catalog.LoadAsync();
        }
        finally
        {
            IsLoading = false;
            ApplyFilter();
        }
    }

    partial void OnSearchChanged(string value) => ApplyFilter();
    partial void OnTypeFilterChanged(string value) => ApplyFilter();

    private void ApplyFilter()
    {
        var q = (Search ?? string.Empty).Trim().ToLowerInvariant();
        Filtered.Clear();

        foreach (var c in _catalog.Items)
        {
            if (TypeFilter != "All" && c.Type.ToString() != TypeFilter) continue;
            if (q.Length > 0 &&
                !c.Name.ToLowerInvariant().Contains(q) &&
                !c.Id.ToLowerInvariant().Contains(q)) continue;

            Filtered.Add(c);
        }
    }

    public bool IsOwned(string id) => _settings.Current.MyLocker.Contains(id);

    [RelayCommand]
    private void ToggleOwned(Cosmetic? cosmetic)
    {
        if (cosmetic is null) return;

        if (IsOwned(cosmetic.Id))
        {
            _settings.Current.MyLocker.Remove(cosmetic.Id);
            OwnedIds.Remove(cosmetic.Id);
        }
        else
        {
            _settings.Current.MyLocker.Add(cosmetic.Id);
            OwnedIds.Add(cosmetic.Id);
        }

        _settings.Save();
        OwnedCount = _settings.Current.MyLocker.Count;
    }

    [RelayCommand]
    private void GoToSwap() => _nav.RequestNavigate("Swap");
}
