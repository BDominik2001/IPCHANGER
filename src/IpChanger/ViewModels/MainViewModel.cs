using System.Collections.ObjectModel;
using IpChanger.Models;
using IpChanger.Services;

namespace IpChanger.ViewModels;

/// <summary>
/// A főablak ViewModelje: presetek listája, adapterválasztás, és a preset
/// alkalmazása. A presetek minden módosítás után automatikusan mentődnek.
/// </summary>
public sealed class MainViewModel : ObservableObject
{
    private readonly PresetStore _store;
    private readonly NetworkAdapterService _network;
    private readonly IDialogService _dialogs;

    private IpPreset? _selectedPreset;
    private NetworkAdapterInfo? _selectedAdapter;
    private string _statusMessage = "Készen áll.";
    private bool _statusIsError;

    public MainViewModel(PresetStore store, NetworkAdapterService network, IDialogService dialogs)
    {
        _store = store;
        _network = network;
        _dialogs = dialogs;

        Presets = new ObservableCollection<IpPreset>(_store.Load());
        Adapters = new ObservableCollection<NetworkAdapterInfo>();

        AddPresetCommand = new RelayCommand(AddPreset);
        EditPresetCommand = new RelayCommand(EditPreset, () => SelectedPreset is not null);
        DuplicatePresetCommand = new RelayCommand(DuplicatePreset, () => SelectedPreset is not null);
        DeletePresetCommand = new RelayCommand(DeletePreset, () => SelectedPreset is not null);
        ApplyPresetCommand = new RelayCommand(ApplyPreset,
            () => SelectedPreset is not null && SelectedAdapter is not null);
        RefreshAdaptersCommand = new RelayCommand(RefreshAdapters);
        ImportCommand = new RelayCommand(Import);
        ExportCommand = new RelayCommand(Export, () => Presets.Count > 0);

        RefreshAdapters();
    }

    public ObservableCollection<IpPreset> Presets { get; }
    public ObservableCollection<NetworkAdapterInfo> Adapters { get; }

    public IpPreset? SelectedPreset
    {
        get => _selectedPreset;
        set
        {
            if (SetProperty(ref _selectedPreset, value))
                RaiseItemCommands();
        }
    }

    public NetworkAdapterInfo? SelectedAdapter
    {
        get => _selectedAdapter;
        set
        {
            if (SetProperty(ref _selectedAdapter, value))
                ApplyPresetCommand.RaiseCanExecuteChanged();
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public bool StatusIsError
    {
        get => _statusIsError;
        private set => SetProperty(ref _statusIsError, value);
    }

    public string StorePath => _store.FilePath;

    public RelayCommand AddPresetCommand { get; }
    public RelayCommand EditPresetCommand { get; }
    public RelayCommand DuplicatePresetCommand { get; }
    public RelayCommand DeletePresetCommand { get; }
    public RelayCommand ApplyPresetCommand { get; }
    public RelayCommand RefreshAdaptersCommand { get; }
    public RelayCommand ImportCommand { get; }
    public RelayCommand ExportCommand { get; }

    private void AddPreset()
    {
        var editorVm = new PresetEditorViewModel(new IpPreset(), isNew: true);
        if (!_dialogs.ShowPresetEditor(editorVm))
            return;

        var created = editorVm.GetResult();
        Presets.Add(created);
        SelectedPreset = created;
        PersistPresets();
        SetStatus($"Preset létrehozva: {created.Name}", isError: false);
    }

    private void EditPreset()
    {
        if (SelectedPreset is null)
            return;

        var editorVm = new PresetEditorViewModel(SelectedPreset, isNew: false);
        if (!_dialogs.ShowPresetEditor(editorVm))
            return;

        // A módosításokat visszaírjuk az eredeti példányba.
        var edited = editorVm.GetResult();
        var current = SelectedPreset;
        var index = Presets.IndexOf(current);
        current.CopyValuesFrom(edited);

        // Az IpPreset nem observable, ezért a kötések frissítéséhez kényszerítjük az
        // újraértékelést: a listaelemet önmagára cseréljük (ListBox kártya), a
        // SelectedPreset kötést pedig null-on át állítjuk vissza (részletező panel).
        _selectedPreset = null;
        OnPropertyChanged(nameof(SelectedPreset));
        if (index >= 0)
            Presets[index] = current;
        _selectedPreset = current;
        OnPropertyChanged(nameof(SelectedPreset));
        RaiseItemCommands();

        PersistPresets();
        SetStatus($"Preset frissítve: {current.Name}", isError: false);
    }

    private void DuplicatePreset()
    {
        if (SelectedPreset is null)
            return;

        var copy = SelectedPreset.Clone();
        copy.Id = Guid.NewGuid();
        copy.Name = $"{SelectedPreset.Name} (másolat)";
        Presets.Add(copy);
        SelectedPreset = copy;
        PersistPresets();
        SetStatus($"Preset duplikálva: {copy.Name}", isError: false);
    }

    private void DeletePreset()
    {
        if (SelectedPreset is null)
            return;

        if (!_dialogs.Confirm($"Biztosan törlöd a(z) \"{SelectedPreset.Name}\" presetet?", "Törlés megerősítése"))
            return;

        var name = SelectedPreset.Name;
        Presets.Remove(SelectedPreset);
        SelectedPreset = Presets.FirstOrDefault();
        PersistPresets();
        SetStatus($"Preset törölve: {name}", isError: false);
    }

    private void ApplyPreset()
    {
        if (SelectedPreset is null || SelectedAdapter is null)
            return;

        if (!_dialogs.Confirm(
                $"A(z) \"{SelectedPreset.Name}\" preset alkalmazása a(z) \"{SelectedAdapter.Name}\" adapterre?",
                "Alkalmazás megerősítése"))
            return;

        var result = _network.ApplyPreset(SelectedAdapter.Name, SelectedPreset);
        SetStatus(result.Message, isError: !result.Success);

        if (result.Success)
            RefreshAdapters();
        else
            _dialogs.ShowMessage(result.Message, "Az alkalmazás sikertelen", isError: true);
    }

    private void RefreshAdapters()
    {
        var previouslySelected = SelectedAdapter?.Name;
        Adapters.Clear();
        foreach (var adapter in _network.GetAdapters())
            Adapters.Add(adapter);

        SelectedAdapter = Adapters.FirstOrDefault(a => a.Name == previouslySelected)
            ?? Adapters.FirstOrDefault(a => a.IsUp)
            ?? Adapters.FirstOrDefault();
    }

    private void Import()
    {
        var path = _dialogs.ShowOpenFileDialog(
            "IP preset fájl (*.json)|*.json|Minden fájl (*.*)|*.*",
            "Presetek importálása");
        if (path is null)
            return;

        try
        {
            var imported = _store.Import(path);
            if (imported.Count == 0)
            {
                SetStatus("A fájl nem tartalmazott presetet.", isError: true);
                return;
            }

            foreach (var preset in imported)
                Presets.Add(preset);

            PersistPresets();
            ExportCommand.RaiseCanExecuteChanged();
            SetStatus($"{imported.Count} preset importálva.", isError: false);
        }
        catch (Exception ex)
        {
            SetStatus($"Az importálás sikertelen: {ex.Message}", isError: true);
            _dialogs.ShowMessage(ex.Message, "Importálási hiba", isError: true);
        }
    }

    private void Export()
    {
        if (Presets.Count == 0)
            return;

        var path = _dialogs.ShowSaveFileDialog(
            "IP preset fájl (*.json)|*.json",
            "Presetek exportálása",
            "ip-presetek.json");
        if (path is null)
            return;

        try
        {
            _store.Export(Presets, path);
            SetStatus($"{Presets.Count} preset exportálva ide: {path}", isError: false);
        }
        catch (Exception ex)
        {
            SetStatus($"Az exportálás sikertelen: {ex.Message}", isError: true);
            _dialogs.ShowMessage(ex.Message, "Exportálási hiba", isError: true);
        }
    }

    private void PersistPresets()
    {
        try
        {
            _store.Save(Presets);
        }
        catch (Exception ex)
        {
            SetStatus($"A presetek mentése sikertelen: {ex.Message}", isError: true);
        }

        ExportCommand.RaiseCanExecuteChanged();
    }

    private void RaiseItemCommands()
    {
        EditPresetCommand.RaiseCanExecuteChanged();
        DuplicatePresetCommand.RaiseCanExecuteChanged();
        DeletePresetCommand.RaiseCanExecuteChanged();
        ApplyPresetCommand.RaiseCanExecuteChanged();
    }

    private void SetStatus(string message, bool isError)
    {
        StatusMessage = message;
        StatusIsError = isError;
    }
}
