using IpChanger.ViewModels;

namespace IpChanger.Services;

/// <summary>
/// A felhasználói felület felugró ablakainak absztrakciója, hogy a ViewModel
/// ne függjön közvetlenül a WPF-től.
/// </summary>
public interface IDialogService
{
    /// <summary>Megnyitja a preset szerkesztőt. True, ha a felhasználó mentett.</summary>
    bool ShowPresetEditor(PresetEditorViewModel viewModel);

    /// <summary>Megnyitja az importálás-áttekintő ablakot. True, ha a felhasználó az Importálást választotta.</summary>
    bool ShowImportReview(ImportReviewViewModel viewModel);

    /// <summary>Megnyitja az exportálás-választó ablakot. True, ha a felhasználó az Exportálást választotta.</summary>
    bool ShowExportSelection(ExportSelectionViewModel viewModel);

    /// <summary>Fájl megnyitási párbeszéd (importhoz). Null, ha megszakították.</summary>
    string? ShowOpenFileDialog(string filter, string title);

    /// <summary>Fájl mentési párbeszéd (exporthoz). Null, ha megszakították.</summary>
    string? ShowSaveFileDialog(string filter, string title, string defaultFileName);

    /// <summary>Igen/Nem megerősítő kérdés.</summary>
    bool Confirm(string message, string title);

    /// <summary>Információs vagy hibaüzenet megjelenítése.</summary>
    void ShowMessage(string message, string title, bool isError = false);

    /// <summary>A Névjegy ablak megjelenítése.</summary>
    void ShowAbout();

    /// <summary>A használati útmutató ablak megjelenítése.</summary>
    void ShowManual();
}
