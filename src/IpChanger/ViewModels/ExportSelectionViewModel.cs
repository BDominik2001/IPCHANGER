using System.Collections.ObjectModel;
using System.ComponentModel;
using IpChanger.Models;
using IpChanger.Services;

namespace IpChanger.ViewModels;

/// <summary>Egy exportálható preset a választólistában, ki-/bejelölhető állapottal.</summary>
public sealed class ExportItemViewModel : ObservableObject
{
    private bool _isSelected = true;

    public ExportItemViewModel(IpPreset preset)
    {
        Preset = preset;
    }

    public IpPreset Preset { get; }
    public string Name => Preset.Name;
    public string Summary => Preset.Summary;

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}

/// <summary>
/// Az exportálás-választó ablak ViewModelje: a felhasználó elemenként jelölheti ki
/// az exportálandó preseteket, és egy gyorsgombbal az összeset ki-/bejelölheti.
/// </summary>
public sealed class ExportSelectionViewModel : ObservableObject
{
    private readonly ILocalizationService _loc;

    public ExportSelectionViewModel(IEnumerable<IpPreset> presets, ILocalizationService loc)
    {
        _loc = loc;

        Items = new ObservableCollection<ExportItemViewModel>();
        foreach (var preset in presets)
        {
            var item = new ExportItemViewModel(preset);
            item.PropertyChanged += OnItemChanged;
            Items.Add(item);
        }

        ToggleAllCommand = new RelayCommand(ToggleAll);
    }

    public ObservableCollection<ExportItemViewModel> Items { get; }

    public RelayCommand ToggleAllCommand { get; }

    /// <summary>A kijelölt elemek száma.</summary>
    public int SelectedCount => Items.Count(i => i.IsSelected);

    /// <summary>Igaz, ha legalább egy elem ki van jelölve (az Export gomb ettől aktív).</summary>
    public bool CanExport => SelectedCount > 0;

    /// <summary>„N kijelölve” felirat.</summary>
    public string SelectedText => _loc.Format("L.Export.Selected", SelectedCount);

    /// <summary>A gyorsgomb felirata (minden kijelölve → törlés, egyébként → összes kijelölése).</summary>
    public string ToggleAllLabel => AllSelected
        ? _loc.Get("L.Export.DeselectAll")
        : _loc.Get("L.Export.SelectAll");

    private bool AllSelected => Items.Count > 0 && SelectedCount == Items.Count;

    private void ToggleAll()
    {
        var target = !AllSelected; // ha nincs mind kijelölve → mind kijelölése, egyébként törlés
        foreach (var item in Items)
            item.IsSelected = target;
    }

    private void OnItemChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(ExportItemViewModel.IsSelected))
            return;

        OnPropertyChanged(nameof(SelectedCount));
        OnPropertyChanged(nameof(CanExport));
        OnPropertyChanged(nameof(SelectedText));
        OnPropertyChanged(nameof(ToggleAllLabel));
    }

    /// <summary>A kijelölt presetek listája (az eredeti sorrendben).</summary>
    public IReadOnlyList<IpPreset> GetSelectedPresets()
        => Items.Where(i => i.IsSelected).Select(i => i.Preset).ToList();
}
