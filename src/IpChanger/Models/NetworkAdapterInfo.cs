namespace IpChanger.Models;

/// <summary>
/// Egy hálózati adapter felhasználó számára megjeleníthető, csak olvasható leírása.
/// </summary>
public sealed class NetworkAdapterInfo
{
    /// <summary>Az adapter rendszer-neve, amit a netsh parancsokhoz használunk (pl. "Ethernet").</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Az adapter felhasználóbarát leírása (pl. gyártó/típus).</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Az aktuális IPv4 cím, ha van (csak megjelenítéshez).</summary>
    public string? CurrentIpv4 { get; init; }

    /// <summary>Igaz, ha az adapter épp működik (Up állapot).</summary>
    public bool IsUp { get; init; }

    /// <summary>Listában megjeleníthető felirat.</summary>
    public string DisplayName =>
        string.IsNullOrWhiteSpace(Description) || Description == Name
            ? Name
            : $"{Name} — {Description}";

    public override string ToString() => DisplayName;
}
