using System.Collections.ObjectModel;
using System.Threading.Tasks;
using FortniteSwapper.Models;

namespace FortniteSwapper.Services;

/// <summary>
/// Shared, in-memory cosmetic catalog loaded once from cache/API and reused by
/// Browse and Swap views.
/// </summary>
public interface ICatalogService
{
    ObservableCollection<Cosmetic> Items { get; }
    Task LoadAsync();
    event System.Action<string>? StatusChanged;
}

public class CatalogService : ICatalogService
{
    private readonly IApiClient _api;
    private readonly ICacheService _cache;
    private readonly ILogService _log;

    public ObservableCollection<Cosmetic> Items { get; } = new();
    public event System.Action<string>? StatusChanged;

    public CatalogService(IApiClient api, ICacheService cache, ILogService log)
    {
        _api = api;
        _cache = cache;
        _log = log;
    }

    public async Task LoadAsync()
    {
        if (Items.Count > 0) return;

        var cached = _cache.LoadCosmetics();
        if (cached is { Count: > 0 })
        {
            foreach (var c in cached) Items.Add(c);
            StatusChanged?.Invoke($"Loaded {Items.Count} cosmetics from local cache.");
        }

        try
        {
            var live = await _api.GetCosmeticsAsync();
            Items.Clear();
            foreach (var c in live) Items.Add(c);
            _cache.SaveCosmetics(live);
            StatusChanged?.Invoke($"Loaded {Items.Count} cosmetics from Fortnite-API.");
        }
        catch (System.Exception ex)
        {
            _log.Error("Failed to load cosmetics from API", ex);
            StatusChanged?.Invoke(cached is null
                ? $"Could not reach Fortnite-API and no cache is available: {ex.Message}"
                : "Fortnite-API unavailable; showing cached cosmetics.");
        }
    }
}
