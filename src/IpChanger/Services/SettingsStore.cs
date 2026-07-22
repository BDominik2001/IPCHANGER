using System.IO;
using System.Text.Json;

namespace IpChanger.Services;

/// <summary>Perzisztens alkalmazásbeállítások (pl. a választott téma).</summary>
public sealed class AppSettings
{
    public AppTheme Theme { get; set; } = AppTheme.Dark;
    public AppLanguage Language { get; set; } = AppLanguage.Hungarian;
}

/// <summary>
/// Az alkalmazásbeállítások betöltése és mentése a felhasználó AppData mappájába.
/// </summary>
public sealed class SettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
    };

    private readonly string _filePath;

    public SettingsStore()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "IpChanger");
        Directory.CreateDirectory(dir);
        _filePath = Path.Combine(dir, "settings.json");
    }

    public AppSettings Load()
    {
        if (!File.Exists(_filePath))
            return new AppSettings();

        try
        {
            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public void Save(AppSettings settings)
    {
        try
        {
            var json = JsonSerializer.Serialize(settings, JsonOptions);
            File.WriteAllText(_filePath, json);
        }
        catch
        {
            // A beállítás mentésének hibája ne akassza meg az alkalmazást.
        }
    }
}
