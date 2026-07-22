using IpChanger.Models;
using IpChanger.Services;

namespace IpChanger.ViewModels;

/// <summary>
/// Egy preset szerkesztésének ViewModelje. Egy piszkozaton dolgozik, így a
/// Mégse gomb nem módosítja az eredeti presetet. Mentéskor validál.
/// </summary>
public sealed class PresetEditorViewModel : ObservableObject
{
    private readonly IpPreset _draft;

    public PresetEditorViewModel(IpPreset source, bool isNew)
    {
        _draft = source.Clone();
        Title = isNew ? "Új preset" : "Preset szerkesztése";
    }

    /// <summary>Az ablak címe.</summary>
    public string Title { get; }

    public string Name
    {
        get => _draft.Name;
        set { _draft.Name = value; OnPropertyChanged(); }
    }

    public bool UseDhcp
    {
        get => _draft.UseDhcp;
        set
        {
            _draft.UseDhcp = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsStatic));
        }
    }

    /// <summary>Kényelmi tulajdonság a statikus mezők engedélyezéséhez a nézetben.</summary>
    public bool IsStatic => !_draft.UseDhcp;

    public string IpAddress
    {
        get => _draft.IpAddress;
        set { _draft.IpAddress = value; OnPropertyChanged(); }
    }

    public string SubnetMask
    {
        get => _draft.SubnetMask;
        set { _draft.SubnetMask = value; OnPropertyChanged(); }
    }

    public string Gateway
    {
        get => _draft.Gateway;
        set { _draft.Gateway = value; OnPropertyChanged(); }
    }

    public bool DnsFromDhcp
    {
        get => _draft.DnsFromDhcp;
        set
        {
            _draft.DnsFromDhcp = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsStaticDns));
        }
    }

    /// <summary>Igaz, ha kézzel adjuk meg a DNS-t (statikus IP és nem DHCP DNS).</summary>
    public bool IsStaticDns => !_draft.DnsFromDhcp;

    public string PreferredDns
    {
        get => _draft.PreferredDns;
        set { _draft.PreferredDns = value; OnPropertyChanged(); }
    }

    public string AlternateDns
    {
        get => _draft.AlternateDns;
        set { _draft.AlternateDns = value; OnPropertyChanged(); }
    }

    public string Notes
    {
        get => _draft.Notes;
        set { _draft.Notes = value; OnPropertyChanged(); }
    }

    /// <summary>Validálja a piszkozatot. Hibaüzenet vagy null (ha érvényes).</summary>
    public string? Validate() => PresetValidator.Validate(_draft);

    /// <summary>A szerkesztés eredménye: a validált piszkozat.</summary>
    public IpPreset GetResult() => _draft;
}
