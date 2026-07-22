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
    private readonly IThemeService _theme;
    private readonly ILocalizationService _loc;

    private IpPreset? _selectedPreset;
    private NetworkAdapterInfo? _selectedAdapter;
    private string _statusMessage;
    private bool _statusIsError;
    private bool _statusIsDefault = true;
    private LanguageOption _selectedLanguage;

    public MainViewModel(PresetStore store, NetworkAdapterService network, IDialogService dialogs,
        IThemeService theme, ILocalizationService loc)
    {
        _store = store;
        _network = network;
        _dialogs = dialogs;
        _theme = theme;
        _loc = loc;

        _statusMessage = loc.Get("L.Status.Ready");

        Presets = new ObservableCollection<IpPreset>(_store.Load());
        Adapters = new ObservableCollection<NetworkAdapterInfo>();

        Languages = new[]
        {
            new LanguageOption(AppLanguage.Hungarian, "Magyar"),
            new LanguageOption(AppLanguage.English, "English"),
        };
        _selectedLanguage = Languages.FirstOrDefault(l => l.Value == loc.Current) ?? Languages[0];

        ToggleThemeCommand = new RelayCommand(ToggleTheme);
        ShowAboutCommand = new RelayCommand(() => _dialogs.ShowAbout());
        ShowManualCommand = new RelayCommand(() => _dialogs.ShowManual());
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

    /// <summary>Igaz, ha épp sötét téma aktív.</summary>
    public bool IsDarkTheme => _theme.Current == AppTheme.Dark;

    /// <summary>A téma-váltó gomb felirata (arra a témára utal, amelyikre váltani lehet).</summary>
    public string ThemeToggleLabel => _loc.Get(IsDarkTheme ? "L.Theme.Light" : "L.Theme.Dark");

    /// <summary>A választható nyelvek.</summary>
    public IReadOnlyList<LanguageOption> Languages { get; }

    /// <summary>Az aktuálisan kiválasztott nyelv. Beállításkor átváltja az egész felületet.</summary>
    public LanguageOption SelectedLanguage
    {
        get => _selectedLanguage;
        set
        {
            if (value is null || !SetProperty(ref _selectedLanguage, value))
                return;

            _loc.Apply(value.Value);

            // A kódból származó, kötött szövegek frissítése (a XAML DynamicResource-ok
            // maguktól frissülnek). A státuszsor csak akkor, ha még az alap üzenet áll.
            OnPropertyChanged(nameof(ThemeToggleLabel));
            if (_statusIsDefault)
                StatusMessage = _loc.Get("L.Status.Ready");
        }
    }

    public RelayCommand ToggleThemeCommand { get; }
    public RelayCommand ShowAboutCommand { get; }
    public RelayCommand ShowManualCommand { get; }
    public RelayCommand AddPresetCommand { get; }
    public RelayCommand EditPresetCommand { get; }
    public RelayCommand DuplicatePresetCommand { get; }
    public RelayCommand DeletePresetCommand { get; }
    public RelayCommand ApplyPresetCommand { get; }
    public RelayCommand RefreshAdaptersCommand { get; }
    public RelayCommand ImportCommand { get; }
    public RelayCommand ExportCommand { get; }

    private void ToggleTheme()
    {
        _theme.Apply(IsDarkTheme ? AppTheme.Light : AppTheme.Dark);
        OnPropertyChanged(nameof(IsDarkTheme));
        OnPropertyChanged(nameof(ThemeToggleLabel));
    }

    private void AddPreset()
    {
        var editorVm = new PresetEditorViewModel(
            new IpPreset { Name = _loc.Get("L.Editor.NewTitle") }, isNew: true, _loc);
        if (!_dialogs.ShowPresetEditor(editorVm))
            return;

        var created = editorVm.GetResult();
        Presets.Add(created);
        SelectedPreset = created;
        PersistPresets();
        SetStatus(_loc.Format("L.Status.Created", created.Name), isError: false);
    }

    private void EditPreset()
    {
        if (SelectedPreset is null)
            return;

        var editorVm = new PresetEditorViewModel(SelectedPreset, isNew: false, _loc);
        if (!_dialogs.ShowPresetEditor(editorVm))
            return;

        // A módosításokat visszaírjuk az eredeti példányba. Mivel az IpPreset
        // observable, a setterek PropertyChanged eseményei automatikusan frissítik
        // a listakártyát és a részletező panelt is.
        var current = SelectedPreset;
        current.CopyValuesFrom(editorVm.GetResult());

        PersistPresets();
        SetStatus(_loc.Format("L.Status.Updated", current.Name), isError: false);
    }

    private void DuplicatePreset()
    {
        if (SelectedPreset is null)
            return;

        var copy = SelectedPreset.Clone();
        copy.Id = Guid.NewGuid();
        copy.Name = $"{SelectedPreset.Name}{_loc.Get("L.Suffix.Copy")}";
        Presets.Add(copy);
        SelectedPreset = copy;
        PersistPresets();
        SetStatus(_loc.Format("L.Status.Duplicated", copy.Name), isError: false);
    }

    private void DeletePreset()
    {
        if (SelectedPreset is null)
            return;

        if (!_dialogs.Confirm(
                _loc.Format("L.Dialog.DeleteConfirm", SelectedPreset.Name),
                _loc.Get("L.Dialog.DeleteTitle")))
            return;

        var name = SelectedPreset.Name;
        Presets.Remove(SelectedPreset);
        SelectedPreset = Presets.FirstOrDefault();
        PersistPresets();
        SetStatus(_loc.Format("L.Status.Deleted", name), isError: false);
    }

    private void ApplyPreset()
    {
        if (SelectedPreset is null || SelectedAdapter is null)
            return;

        if (!_dialogs.Confirm(
                _loc.Format("L.Dialog.ApplyConfirm", SelectedPreset.Name, SelectedAdapter.Name),
                _loc.Get("L.Dialog.ApplyTitle")))
            return;

        var result = _network.ApplyPreset(SelectedAdapter.Name, SelectedPreset);
        SetStatus(result.Message, isError: !result.Success);

        if (result.Success)
            RefreshAdapters();
        else
            _dialogs.ShowMessage(result.Message, _loc.Get("L.Dialog.ApplyFailedTitle"), isError: true);
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
            _loc.Get("L.File.Filter"),
            _loc.Get("L.File.ImportTitle"));
        if (path is null)
            return;

        try
        {
            var imported = _store.Import(path);
            if (imported.Count == 0)
            {
                SetStatus(_loc.Get("L.Status.ImportEmpty"), isError: true);
                return;
            }

            // Áttekintő ablak: a felhasználó elemenként dönthet a névütközésekről.
            var review = new ImportReviewViewModel(Presets, imported, _loc);
            if (!_dialogs.ShowImportReview(review))
                return; // megszakítva

            int added = 0, overwritten = 0, skipped = 0;
            foreach (var item in review.Incoming)
            {
                switch (item.ActionValue)
                {
                    case ImportAction.Skip:
                        skipped++;
                        break;

                    case ImportAction.Add:
                        Presets.Add(item.Preset);
                        added++;
                        break;

                    case ImportAction.KeepBoth:
                        item.Preset.Name = UniqueImportedName(item.Preset.Name);
                        Presets.Add(item.Preset);
                        added++;
                        break;

                    case ImportAction.Overwrite:
                        var existing = Presets.FirstOrDefault(p => NameEquals(p.Name, item.Preset.Name));
                        if (existing is not null)
                        {
                            existing.CopyValuesFrom(item.Preset);
                            overwritten++;
                        }
                        else
                        {
                            Presets.Add(item.Preset);
                            added++;
                        }
                        break;
                }
            }

            PersistPresets();
            ExportCommand.RaiseCanExecuteChanged();
            SetStatus(_loc.Format("L.Status.ImportResult", added, overwritten, skipped), isError: false);
        }
        catch (Exception ex)
        {
            SetStatus(_loc.Format("L.Status.ImportFailed", ex.Message), isError: true);
            _dialogs.ShowMessage(ex.Message, _loc.Get("L.Dialog.ImportErrorTitle"), isError: true);
        }
    }

    /// <summary>Két preset-név egyezésének vizsgálata (kis/nagybetűre érzéketlen, szóközök levágva).</summary>
    private static bool NameEquals(string? a, string? b)
        => ImportReviewViewModel.Normalize(a) == ImportReviewViewModel.Normalize(b);

    /// <summary>Egyedi név előállítása „mindkettő megtartása” esetén: „név (importált)”, „név (importált 2)”, …</summary>
    private string UniqueImportedName(string baseName)
    {
        var word = _loc.Get("L.Import.RenameWord");
        var candidate = $"{baseName} ({word})";
        var n = 2;
        while (Presets.Any(p => NameEquals(p.Name, candidate)))
            candidate = $"{baseName} ({word} {n++})";
        return candidate;
    }

    private void Export()
    {
        if (Presets.Count == 0)
            return;

        var path = _dialogs.ShowSaveFileDialog(
            _loc.Get("L.File.ExportFilter"),
            _loc.Get("L.File.ExportTitle"),
            _loc.Get("L.File.ExportDefault"));
        if (path is null)
            return;

        try
        {
            _store.Export(Presets, path);
            SetStatus(_loc.Format("L.Status.Exported", Presets.Count, path), isError: false);
        }
        catch (Exception ex)
        {
            SetStatus(_loc.Format("L.Status.ExportFailed", ex.Message), isError: true);
            _dialogs.ShowMessage(ex.Message, _loc.Get("L.Dialog.ExportErrorTitle"), isError: true);
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
            SetStatus(_loc.Format("L.Status.SaveFailed", ex.Message), isError: true);
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
        _statusIsDefault = false;
        StatusMessage = message;
        StatusIsError = isError;
    }
}
