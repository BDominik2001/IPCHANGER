using System.Windows;
using IpChanger.Services;

namespace IpChanger.Views;

/// <summary>
/// Az ILocalizationService WPF-alapú implementációja. A megfelelő nyelvi
/// erőforrásszótárat helyezi be az alkalmazás összevont erőforrásai közé
/// (a XAML DynamicResource-ok ezt látják), és elmenti a választást.
/// </summary>
public sealed class LocalizationService : ILocalizationService
{
    private readonly SettingsStore _settings;
    private ResourceDictionary? _currentLangDict;

    public LocalizationService(SettingsStore settings)
    {
        _settings = settings;
    }

    public AppLanguage Current { get; private set; } = AppLanguage.Hungarian;

    public void Apply(AppLanguage language)
    {
        var source = language == AppLanguage.English
            ? "Themes/Lang.English.xaml"
            : "Themes/Lang.Hungarian.xaml";

        var newDict = new ResourceDictionary { Source = new Uri(source, UriKind.Relative) };

        var merged = Application.Current.Resources.MergedDictionaries;
        if (_currentLangDict is not null)
            merged.Remove(_currentLangDict);

        merged.Insert(0, newDict);
        _currentLangDict = newDict;

        Current = language;

        var settings = _settings.Load();
        settings.Language = language;
        _settings.Save(settings);
    }

    public string Get(string key)
    {
        var value = Application.Current?.TryFindResource(key);
        return value as string ?? key;
    }

    public string Format(string key, params object?[] args)
        => string.Format(Get(key), args);
}
