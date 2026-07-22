namespace IpChanger.Services;

/// <summary>Az alkalmazás által támogatott nyelvek.</summary>
public enum AppLanguage
{
    Hungarian,
    English,
}

/// <summary>
/// A felület nyelvének futásidejű váltása. A XAML a nyelvi kulcsokra
/// DynamicResource-szal hivatkozik, a kódból pedig a <see cref="Get"/> olvassa
/// ki a lokalizált szövegeket – így nyelvváltáskor minden azonnal frissül.
/// </summary>
public interface ILocalizationService
{
    /// <summary>Az aktuális nyelv.</summary>
    AppLanguage Current { get; }

    /// <summary>A megadott nyelv alkalmazása az egész alkalmazásra.</summary>
    void Apply(AppLanguage language);

    /// <summary>Egy lokalizált szöveg lekérése kulcs alapján (hiány esetén a kulcsot adja vissza).</summary>
    string Get(string key);

    /// <summary>Lokalizált, formázott szöveg (string.Format a lekért mintára).</summary>
    string Format(string key, params object?[] args);
}
