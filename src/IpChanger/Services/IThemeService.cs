namespace IpChanger.Services;

/// <summary>Az alkalmazás megjelenési témái.</summary>
public enum AppTheme
{
    Dark,
    Light,
}

/// <summary>
/// A téma futásidejű váltásának absztrakciója, hogy a ViewModel ne függjön a WPF-től.
/// </summary>
public interface IThemeService
{
    /// <summary>Az aktuálisan alkalmazott téma.</summary>
    AppTheme Current { get; }

    /// <summary>A megadott téma alkalmazása az egész alkalmazásra.</summary>
    void Apply(AppTheme theme);
}
