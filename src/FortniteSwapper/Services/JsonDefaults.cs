using System.Text.Json;

namespace FortniteSwapper.Services;

/// <summary>
/// Shared JSON options. External inputs (Fortnite-API responses, mapping files) use
/// camelCase keys, so we deserialise case-insensitively while keeping the models readable
/// (PascalCase). Used everywhere we parse JSON.
/// </summary>
public static class JsonDefaults
{
    public static JsonSerializerOptions Options { get; } = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };
}
