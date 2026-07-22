using System.Text.Json.Serialization;

namespace IpChanger.Models;

/// <summary>
/// Egy elmenthető IP-konfiguráció (preset). Statikus IPv4 beállításokat vagy
/// DHCP-t ír le, opcionális DNS beállításokkal és felhasználói megjegyzéssel.
/// </summary>
public sealed class IpPreset
{
    /// <summary>Egyedi azonosító, hogy a lista elemei stabilan azonosíthatók legyenek.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>A preset megjelenített neve.</summary>
    public string Name { get; set; } = "Új preset";

    /// <summary>Ha igaz, a preset alkalmazása DHCP-re állítja az adaptert (a statikus mezők figyelmen kívül maradnak).</summary>
    public bool UseDhcp { get; set; }

    /// <summary>Statikus IPv4 cím (pl. 192.168.0.10).</summary>
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>Alhálózati maszk (pl. 255.255.255.0).</summary>
    public string SubnetMask { get; set; } = "255.255.255.0";

    /// <summary>Alapértelmezett átjáró (opcionális).</summary>
    public string Gateway { get; set; } = string.Empty;

    /// <summary>Ha igaz, a DNS-t is a DHCP adja (statikus DNS mezők figyelmen kívül maradnak).</summary>
    public bool DnsFromDhcp { get; set; }

    /// <summary>Elsődleges DNS szerver (opcionális).</summary>
    public string PreferredDns { get; set; } = string.Empty;

    /// <summary>Másodlagos DNS szerver (opcionális).</summary>
    public string AlternateDns { get; set; } = string.Empty;

    /// <summary>Szabad szöveges megjegyzés a presethez.</summary>
    public string Notes { get; set; } = string.Empty;

    /// <summary>Rövid, listában megjeleníthető összefoglaló a beállításról.</summary>
    [JsonIgnore]
    public string Summary => UseDhcp
        ? "DHCP (automatikus)"
        : string.IsNullOrWhiteSpace(IpAddress) ? "Statikus (nincs cím megadva)" : IpAddress;

    /// <summary>Mély másolat készítése (szerkesztés közbeni piszkozathoz).</summary>
    public IpPreset Clone() => new()
    {
        Id = Id,
        Name = Name,
        UseDhcp = UseDhcp,
        IpAddress = IpAddress,
        SubnetMask = SubnetMask,
        Gateway = Gateway,
        DnsFromDhcp = DnsFromDhcp,
        PreferredDns = PreferredDns,
        AlternateDns = AlternateDns,
        Notes = Notes,
    };

    /// <summary>A megadott forrás mezőit másolja ebbe a példányba (Id kivételével).</summary>
    public void CopyValuesFrom(IpPreset source)
    {
        Name = source.Name;
        UseDhcp = source.UseDhcp;
        IpAddress = source.IpAddress;
        SubnetMask = source.SubnetMask;
        Gateway = source.Gateway;
        DnsFromDhcp = source.DnsFromDhcp;
        PreferredDns = source.PreferredDns;
        AlternateDns = source.AlternateDns;
        Notes = source.Notes;
    }
}
