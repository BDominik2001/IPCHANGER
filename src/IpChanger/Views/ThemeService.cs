using System.Windows;
using IpChanger.Services;

namespace IpChanger.Views;

/// <summary>
/// Az IThemeService WPF-alapú implementációja. A megfelelő téma-erőforrásszótárat
/// (DarkTheme/LightTheme) helyezi be az alkalmazás összevont erőforrásai közé,
/// és elmenti a választást. A stílusok DynamicResource-szal hivatkoznak az
/// ecsetekre, így a váltás azonnal érvényesül a teljes felületen.
/// </summary>
public sealed class ThemeService : IThemeService
{
    private readonly SettingsStore _settings;
    private ResourceDictionary? _currentThemeDict;

    public ThemeService(SettingsStore settings)
    {
        _settings = settings;
    }

    public AppTheme Current { get; private set; } = AppTheme.Dark;

    public void Apply(AppTheme theme)
    {
        var source = theme == AppTheme.Light
            ? "Themes/LightTheme.xaml"
            : "Themes/DarkTheme.xaml";

        var newDict = new ResourceDictionary { Source = new Uri(source, UriKind.Relative) };

        var merged = Application.Current.Resources.MergedDictionaries;
        if (_currentThemeDict is not null)
            merged.Remove(_currentThemeDict);

        // A témát az összevont szótárak elejére tesszük, hogy a DynamicResource
        // feloldás elsőként ezt találja meg.
        merged.Insert(0, newDict);
        _currentThemeDict = newDict;

        Current = theme;

        var settings = _settings.Load();
        settings.Theme = theme;
        _settings.Save(settings);
    }
}
