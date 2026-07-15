using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using FortniteSwapper.Models;

namespace FortniteSwapper.Services;

public interface ICacheService
{
    List<Cosmetic>? LoadCosmetics();
    void SaveCosmetics(List<Cosmetic> cosmetics);
}

public class CacheService : ICacheService
{
    private readonly string _file;

    public CacheService(string? cacheFile = null)
    {
        _file = cacheFile ?? Paths.CacheFile;
    }

    public List<Cosmetic>? LoadCosmetics()
    {
        try
        {
            if (!File.Exists(_file)) return null;
            var list = JsonSerializer.Deserialize<List<Cosmetic>>(File.ReadAllText(_file));
            return list is { Count: > 0 } ? list : null;
        }
        catch
        {
            return null;
        }
    }

    public void SaveCosmetics(List<Cosmetic> cosmetics)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_file)!);
            File.WriteAllText(_file, JsonSerializer.Serialize(cosmetics, new JsonSerializerOptions { WriteIndented = false }));
        }
        catch
        {
            // Non-fatal.
        }
    }
}
