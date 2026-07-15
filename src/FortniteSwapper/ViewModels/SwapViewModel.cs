using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FortniteSwapper.Models;
using FortniteSwapper.Services;
using System.Collections.ObjectModel;

namespace FortniteSwapper.ViewModels;

public partial class SwapViewModel : ViewModelBase
{
    private readonly ISettingsService _settings;
    private readonly IMappingService _mappings;
    private readonly IFortniteLocator _locator;
    private readonly ISwapService _swap;
    private readonly ICatalogService _catalog;
    private readonly ILogService _log;

    [ObservableProperty] private Cosmetic? _source;
    [ObservableProperty] private Cosmetic? _target;
    [ObservableProperty] private ObservableCollection<Cosmetic> _ownedCosmetics = new();
    [ObservableProperty] private string _statusMessage = "Pick a cosmetic you own as the source, then a look to swap it with.";
    [ObservableProperty] private bool _canApply;
    [ObservableProperty] private bool _mappingReady;
    [ObservableProperty] private ObservableCollection<string> _logLines = new();
    [ObservableProperty] private ObservableCollection<SwapRecord> _appliedSwaps = new();
    [ObservableProperty] private string _versionsNote = string.Empty;

    public ObservableCollection<Cosmetic> Catalog => _catalog.Items;

    public override string Title => "Swap";

    public SwapViewModel(ISettingsService settings, IMappingService mappings, IFortniteLocator locator,
                         ISwapService swap, ICatalogService catalog, ILogService log)
    {
        _settings = settings;
        _mappings = mappings;
        _locator = locator;
        _swap = swap;
        _catalog = catalog;
        _log = log;

        _log.EntryAdded += line => LogLines.Add(line);
        _catalog.Items.CollectionChanged += (_, _) => RefreshOwned();
        RefreshMappings();
        RefreshOwned();
    }

    public void OnNavigatedTo()
    {
        RefreshMappings();
        RefreshOwned();
        RefreshApplied();
    }

    private void RefreshMappings()
    {
        var version = ResolveVersion();
        MappingReady = _mappings.LoadForVersion(version);
        VersionsNote = MappingReady
            ? $"Mapping loaded for version '{version}' ({_mappings.Coverage} entries)."
            : $"No mapping for version '{version}'. Import the correct mapping in Settings before swapping.";
        UpdateCanApply();
    }

    private string ResolveVersion()
    {
        var v = _settings.Current.SelectedVersion;
        if (string.IsNullOrWhiteSpace(v)) v = _locator.DetectVersion(_settings.Current.InstallPath) ?? string.Empty;
        return v;
    }

    public void RefreshOwned()
    {
        OwnedCosmetics.Clear();
        foreach (var c in _catalog.Items)
        {
            if (_settings.Current.MyLocker.Contains(c.Id)) OwnedCosmetics.Add(c);
        }
        UpdateCanApply();
    }

    public void RefreshApplied() => AppliedSwaps = new ObservableCollection<SwapRecord>(_swap.GetAppliedSwaps());

    partial void OnSourceChanged(Cosmetic? value) => UpdateCanApply();
    partial void OnTargetChanged(Cosmetic? value) => UpdateCanApply();

    private void UpdateCanApply()
    {
        CanApply = Source is not null &&
                   Target is not null &&
                   MappingReady &&
                   _mappings.HasEntry(Source.Id) &&
                   _mappings.HasEntry(Target.Id);
    }

    [RelayCommand]
    private void Apply()
    {
        if (Source is null || Target is null)
        {
            StatusMessage = "Select both a source (owned) cosmetic and a target look.";
            return;
        }

        if (!MappingReady || !_mappings.HasEntry(Source.Id) || !_mappings.HasEntry(Target.Id))
        {
            StatusMessage = "Mapping data is missing for this version. Import it in Settings.";
            return;
        }

        var version = ResolveVersion();
        var result = _swap.ApplySwap(Source.Id, Target.Id, Source.Name, Target.Name, version, _settings.Current.InstallPath);
        StatusMessage = result.Message;
        if (result.Success)
        {
            RefreshApplied();
            Source = null;
            Target = null;
        }
    }

    [RelayCommand]
    private void RestoreAll()
    {
        var result = _swap.RestoreAll(_settings.Current.InstallPath);
        StatusMessage = result.Message;
        RefreshApplied();
        RefreshMappings();
    }

    [RelayCommand]
    private void Restore(SwapRecord? record)
    {
        if (record is null) return;
        var result = _swap.RestoreSwap(record, _settings.Current.InstallPath);
        StatusMessage = result.Message;
        RefreshApplied();
    }
}
