namespace FortniteSwapper.Models;

/// <summary>
/// A single Fortnite cosmetic as presented in the UI. Source-agnostic:
/// mapped from the Fortnite-API response by <see cref="Models.Api.ApiCosmeticExtensions"/>.
/// </summary>
public class Cosmetic
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public CosmeticType Type { get; set; }
    public string Rarity { get; set; } = string.Empty;
    public string? Set { get; set; }
    public string? Series { get; set; }
    public string? SmallIconUrl { get; set; }
    public string? IconUrl { get; set; }
    public string? FeaturedUrl { get; set; }
}
