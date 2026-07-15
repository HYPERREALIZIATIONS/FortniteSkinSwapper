using FortniteSwapper.Models;

namespace FortniteSwapper.Models.Api;

public class ApiResponse<T>
{
    public int Status { get; set; }
    public T Data { get; set; } = default!;
}

public class ApiCosmetic
{
    public ApiValue? Type { get; set; }
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ApiValue? Rarity { get; set; }
    public ApiValue? Set { get; set; }
    public ApiValue? Series { get; set; }
    public ApiImages? Images { get; set; }
}

public class ApiValue
{
    public string Value { get; set; } = string.Empty;
    public string DisplayValue { get; set; } = string.Empty;
    public string BackendValue { get; set; } = string.Empty;
}

public class ApiImages
{
    public string? SmallIcon { get; set; }
    public string? Icon { get; set; }
    public string? Featured { get; set; }
}

public static class ApiCosmeticExtensions
{
    public static Cosmetic ToCosmetic(this ApiCosmetic a)
    {
        var type = a.Type?.Value?.ToLowerInvariant() switch
        {
            "outfit" => CosmeticType.Outfit,
            "backpack" => CosmeticType.Backpack,
            "pickaxe" => CosmeticType.Pickaxe,
            "emote" => CosmeticType.Emote,
            "glider" => CosmeticType.Glider,
            "wrap" => CosmeticType.Wrap,
            "contrail" => CosmeticType.Contrail,
            "loadingscreen" => CosmeticType.LoadingScreen,
            "music" => CosmeticType.Music,
            _ => CosmeticType.Other
        };

        return new Cosmetic
        {
            Id = a.Id,
            Name = a.Name,
            Description = a.Description,
            Type = type,
            Rarity = a.Rarity?.Value ?? string.Empty,
            Set = a.Set?.Value,
            Series = a.Series?.Value,
            SmallIconUrl = a.Images?.SmallIcon,
            IconUrl = a.Images?.Icon,
            FeaturedUrl = a.Images?.Featured
        };
    }
}
