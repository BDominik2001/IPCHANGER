using IpChanger.Services;

namespace IpChanger.ViewModels;

/// <summary>
/// Egy nyelvválasztó lista eleme. A megjelenített név önmagát nevezi meg
/// (Magyar / English), ezért nem szorul lokalizációra.
/// </summary>
public sealed class LanguageOption
{
    public LanguageOption(AppLanguage value, string display)
    {
        Value = value;
        Display = display;
    }

    public AppLanguage Value { get; }
    public string Display { get; }

    public override string ToString() => Display;
}
