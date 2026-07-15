using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using FortniteSwapper.Models;

namespace FortniteSwapper.Services;

public interface IMappingService
{
    MappingFile? Current { get; }
    string LoadedVersion { get; }
    int Coverage { get; }
    bool LoadForVersion(string version);
    bool HasEntry(string cosmeticId);
    MappingEntry? GetEntry(string cosmeticId);
    void Import(string jsonContent);
    void AddOrUpdate(string cosmeticId, MappingEntry entry);
    void Remove(string cosmeticId);
    void Save();
}

public class MappingService : IMappingService
{
    private readonly string _root;
    private MappingFile? _current;

    public MappingService(string? root = null)
    {
        _root = root ?? Paths.MappingsDir;
        Directory.CreateDirectory(_root);
    }

    public MappingFile? Current => _current;
    public string LoadedVersion => _current?.Version ?? string.Empty;
    public int Coverage => _current?.Cosmetics.Count ?? 0;

    public bool LoadForVersion(string version)
    {
        var file = Path.Combine(_root, $"{Sanitize(version)}.mappings.json");
        if (!File.Exists(file))
        {
            _current = null;
            return false;
        }

        try
        {
            _current = JsonSerializer.Deserialize<MappingFile>(File.ReadAllText(file), JsonDefaults.Options);
            return _current is not null;
        }
        catch
        {
            _current = null;
            return false;
        }
    }

    public bool HasEntry(string cosmeticId) => _current?.Cosmetics.ContainsKey(cosmeticId) == true;

    public MappingEntry? GetEntry(string cosmeticId) =>
        _current?.Cosmetics.TryGetValue(cosmeticId, out var e) == true ? e : null;

    public void Import(string jsonContent)
    {
        var file = JsonSerializer.Deserialize<MappingFile>(jsonContent, JsonDefaults.Options)
                   ?? throw new InvalidOperationException("Invalid mapping JSON.");

        if (string.IsNullOrWhiteSpace(file.Version))
            throw new InvalidOperationException("Mapping JSON must include a 'version'.");

        _current = file;
        Save();
    }

    public void AddOrUpdate(string cosmeticId, MappingEntry entry)
    {
        _current ??= new MappingFile();
        _current.Cosmetics[cosmeticId] = entry;
    }

    public void Remove(string cosmeticId) => _current?.Cosmetics.Remove(cosmeticId);

    public void Save()
    {
        if (_current is null) return;
        Directory.CreateDirectory(_root);
        var file = Path.Combine(_root, $"{Sanitize(_current.Version)}.mappings.json");
        File.WriteAllText(file, JsonSerializer.Serialize(_current, JsonDefaults.Options));
    }

    private static string Sanitize(string version) =>
        string.Concat((version ?? "unknown").Where(c => char.IsLetterOrDigit(c) || c == '.' || c == '-' || c == '_'));
}
