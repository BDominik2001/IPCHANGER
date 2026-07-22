using System.IO;
using System.Text.Json;
using IpChanger.Models;

namespace IpChanger.Services;

/// <summary>
/// A presetek betöltéséért és mentéséért felel. A presetek a felhasználó
/// AppData mappájában, JSON formátumban tárolódnak. Ugyanez a formátum
/// használatos importáláshoz/exportáláshoz is.
/// </summary>
public sealed class PresetStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
    };

    private readonly string _filePath;

    public PresetStore()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "IpChanger");
        Directory.CreateDirectory(dir);
        _filePath = Path.Combine(dir, "presets.json");
    }

    /// <summary>A tárfájl teljes elérési útja (megjelenítéshez).</summary>
    public string FilePath => _filePath;

    /// <summary>Betölti a presetek listáját. Hiányzó vagy sérült fájl esetén üres listát ad.</summary>
    public List<IpPreset> Load()
    {
        if (!File.Exists(_filePath))
            return new List<IpPreset>();

        try
        {
            var json = File.ReadAllText(_filePath);
            return Deserialize(json);
        }
        catch
        {
            // Sérült fájl esetén ne dőljön el az alkalmazás; üres listával indulunk.
            return new List<IpPreset>();
        }
    }

    /// <summary>Elmenti a presetek listáját a tárfájlba.</summary>
    public void Save(IEnumerable<IpPreset> presets)
    {
        var json = JsonSerializer.Serialize(presets.ToList(), JsonOptions);
        File.WriteAllText(_filePath, json);
    }

    /// <summary>Presetek exportálása a megadott fájlba.</summary>
    public void Export(IEnumerable<IpPreset> presets, string path)
    {
        var json = JsonSerializer.Serialize(presets.ToList(), JsonOptions);
        File.WriteAllText(path, json);
    }

    /// <summary>
    /// Presetek importálása a megadott fájlból. Az importált presetek új Id-t kapnak,
    /// hogy ne ütközzenek a meglévőkkel.
    /// </summary>
    public List<IpPreset> Import(string path)
    {
        var json = File.ReadAllText(path);
        var imported = Deserialize(json);
        foreach (var preset in imported)
            preset.Id = Guid.NewGuid();
        return imported;
    }

    private static List<IpPreset> Deserialize(string json)
    {
        var result = JsonSerializer.Deserialize<List<IpPreset>>(json, JsonOptions);
        return result ?? new List<IpPreset>();
    }
}
