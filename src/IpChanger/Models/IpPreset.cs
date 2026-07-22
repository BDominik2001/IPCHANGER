using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace IpChanger.Models;

/// <summary>
/// Egy elmenthető IP-konfiguráció (preset). Statikus IPv4 beállításokat vagy
/// DHCP-t ír le, opcionális DNS beállításokkal és felhasználói megjegyzéssel.
/// INotifyPropertyChanged-et implementál, hogy a felület (lista és részletező)
/// szerkesztés után automatikusan frissüljön.
/// </summary>
public sealed class IpPreset : INotifyPropertyChanged
{
    private string _name = "Új preset";
    private bool _useDhcp;
    private string _ipAddress = string.Empty;
    private string _subnetMask = "255.255.255.0";
    private string _gateway = string.Empty;
    private bool _dnsFromDhcp;
    private string _preferredDns = string.Empty;
    private string _alternateDns = string.Empty;
    private string _notes = string.Empty;

    /// <summary>Egyedi azonosító, hogy a lista elemei stabilan azonosíthatók legyenek.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>A preset megjelenített neve.</summary>
    public string Name
    {
        get => _name;
        set => Set(ref _name, value);
    }

    /// <summary>Ha igaz, a preset alkalmazása DHCP-re állítja az adaptert (a statikus mezők figyelmen kívül maradnak).</summary>
    public bool UseDhcp
    {
        get => _useDhcp;
        set { if (Set(ref _useDhcp, value)) OnPropertyChanged(nameof(Summary)); }
    }

    /// <summary>Statikus IPv4 cím (pl. 192.168.0.10).</summary>
    public string IpAddress
    {
        get => _ipAddress;
        set { if (Set(ref _ipAddress, value)) OnPropertyChanged(nameof(Summary)); }
    }

    /// <summary>Alhálózati maszk (pl. 255.255.255.0).</summary>
    public string SubnetMask
    {
        get => _subnetMask;
        set => Set(ref _subnetMask, value);
    }

    /// <summary>Alapértelmezett átjáró (opcionális).</summary>
    public string Gateway
    {
        get => _gateway;
        set => Set(ref _gateway, value);
    }

    /// <summary>Ha igaz, a DNS-t is a DHCP adja (statikus DNS mezők figyelmen kívül maradnak).</summary>
    public bool DnsFromDhcp
    {
        get => _dnsFromDhcp;
        set => Set(ref _dnsFromDhcp, value);
    }

    /// <summary>Elsődleges DNS szerver (opcionális).</summary>
    public string PreferredDns
    {
        get => _preferredDns;
        set => Set(ref _preferredDns, value);
    }

    /// <summary>Másodlagos DNS szerver (opcionális).</summary>
    public string AlternateDns
    {
        get => _alternateDns;
        set => Set(ref _alternateDns, value);
    }

    /// <summary>Szabad szöveges megjegyzés a presethez.</summary>
    public string Notes
    {
        get => _notes;
        set => Set(ref _notes, value);
    }

    /// <summary>Rövid, listában megjeleníthető összefoglaló (nyelvfüggetlen: az IP címet,
    /// DHCP esetén "DHCP"-t, hiányzó statikus cím esetén "—"-t mutat).</summary>
    [JsonIgnore]
    public string Summary => UseDhcp
        ? "DHCP"
        : string.IsNullOrWhiteSpace(IpAddress) ? "—" : IpAddress;

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

    /// <summary>A megadott forrás mezőit másolja ebbe a példányba (Id kivételével). A setterek
    /// PropertyChanged eseményt váltanak ki, így a felület automatikusan frissül.</summary>
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

    public event PropertyChangedEventHandler? PropertyChanged;

    private bool Set<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
