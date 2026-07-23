using System.Collections.ObjectModel;
using IpChanger.Models;
using IpChanger.Services;

namespace IpChanger.ViewModels;

/// <summary>Egy importálandó preset kezelésének módja névütközés esetén.</summary>
public enum ImportAction
{
    Add,        // hozzáadás (nincs ütközés)
    Overwrite,  // a meglévő azonos nevű felülírása
    KeepBoth,   // mindkettő megtartása (az importált átnevezésével)
    Skip,       // kihagyás
}

/// <summary>Egy választható művelet a legördülőhöz (lokalizált felirattal).</summary>
public sealed class ImportActionOption
{
    public ImportActionOption(ImportAction value, string display)
    {
        Value = value;
        Display = display;
    }

    public ImportAction Value { get; }
    public string Display { get; }
    public override string ToString() => Display;
}

/// <summary>A jobb oldali lista egy eleme: egy importálandó preset + a hozzá választott művelet.</summary>
public sealed class ImportItemViewModel : ObservableObject
{
    private ImportActionOption _selectedAction;

    public ImportItemViewModel(IpPreset preset, bool isConflict, IReadOnlyList<ImportActionOption> actions)
    {
        Preset = preset;
        IsConflict = isConflict;
        AvailableActions = actions;
        // Alapértelmezett: ütközésnél Felülírás (jól látható „csere”), egyébként Hozzáadás.
        _selectedAction = actions[0];
    }

    public IpPreset Preset { get; }
    public bool IsConflict { get; }
    public IReadOnlyList<ImportActionOption> AvailableActions { get; }

    public string Name => Preset.Name;
    public string Summary => Preset.Summary;

    public ImportActionOption SelectedAction
    {
        get => _selectedAction;
        set
        {
            if (SetProperty(ref _selectedAction, value))
                OnPropertyChanged(nameof(ActionValue));
        }
    }

    /// <summary>A kiválasztott művelet enum értéke (a nézet ez alapján színez).</summary>
    public ImportAction ActionValue => _selectedAction.Value;
}

/// <summary>A bal oldali lista egy eleme: egy jelenlegi preset (csak megjelenítéshez).</summary>
public sealed class ExistingPresetRow
{
    public ExistingPresetRow(string name, string summary, bool hasIncomingMatch)
    {
        Name = name;
        Summary = summary;
        HasIncomingMatch = hasIncomingMatch;
    }

    public string Name { get; }
    public string Summary { get; }

    /// <summary>Igaz, ha van azonos nevű importálandó elem (vizuális jelöléshez).</summary>
    public bool HasIncomingMatch { get; }
}

/// <summary>
/// Az importálás-áttekintő ablak ViewModelje: bal oldalt a jelenlegi presetek,
/// jobb oldalt az importálandók, az ütközések jelölésével és elemenként választható
/// művelettel.
/// </summary>
public sealed class ImportReviewViewModel : ObservableObject
{
    public ImportReviewViewModel(IEnumerable<IpPreset> existing, IReadOnlyList<IpPreset> incoming, ILocalizationService loc)
    {
        var existingList = existing.ToList();
        var existingNames = existingList
            .Select(p => Normalize(p.Name))
            .ToHashSet();

        var incomingNames = incoming
            .Select(p => Normalize(p.Name))
            .ToHashSet();

        // Jobb oldal: importálandók
        var addLabel = new ImportActionOption(ImportAction.Add, loc.Get("L.Import.Action.Add"));
        var overwriteLabel = new ImportActionOption(ImportAction.Overwrite, loc.Get("L.Import.Action.Overwrite"));
        var keepBothLabel = new ImportActionOption(ImportAction.KeepBoth, loc.Get("L.Import.Action.KeepBoth"));
        var skipLabel = new ImportActionOption(ImportAction.Skip, loc.Get("L.Import.Action.Skip"));

        var conflictCount = 0;
        Incoming = new ObservableCollection<ImportItemViewModel>();
        foreach (var preset in incoming)
        {
            var isConflict = existingNames.Contains(Normalize(preset.Name));
            if (isConflict)
                conflictCount++;

            var actions = isConflict
                ? new[] { overwriteLabel, keepBothLabel, skipLabel }
                : new[] { addLabel, skipLabel };

            Incoming.Add(new ImportItemViewModel(preset, isConflict, actions));
        }

        // Bal oldal: jelenlegi presetek
        Existing = new ObservableCollection<ExistingPresetRow>(
            existingList.Select(p => new ExistingPresetRow(
                p.Name, p.Summary, incomingNames.Contains(Normalize(p.Name)))));

        SummaryText = loc.Format("L.Import.Summary", incoming.Count, conflictCount);
    }

    public ObservableCollection<ExistingPresetRow> Existing { get; }
    public ObservableCollection<ImportItemViewModel> Incoming { get; }
    public string SummaryText { get; }

    /// <summary>Igaz, ha még nincs egyetlen jelenlegi preset sem (üzenet megjelenítéséhez).</summary>
    public bool HasNoExisting => Existing.Count == 0;

    /// <summary>Név normalizálása az ütközés-ellenőrzéshez (kis/nagybetű és külső szóközök figyelmen kívül).</summary>
    public static string Normalize(string? name)
        => (name ?? string.Empty).Trim().ToLowerInvariant();
}
