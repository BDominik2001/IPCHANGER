using System.Windows;
using IpChanger.Services;
using IpChanger.ViewModels;
using Microsoft.Win32;

namespace IpChanger.Views;

/// <summary>
/// Az IDialogService WPF-alapú implementációja (fájl párbeszédek, üzenetablakok,
/// preset szerkesztő ablak).
/// </summary>
public sealed class DialogService : IDialogService
{
    private static Window? OwnerWindow => Application.Current?.Windows
        .OfType<Window>()
        .FirstOrDefault(w => w.IsActive) ?? Application.Current?.MainWindow;

    public bool ShowPresetEditor(PresetEditorViewModel viewModel)
    {
        var window = new PresetEditorWindow
        {
            DataContext = viewModel,
            Owner = OwnerWindow,
        };
        return window.ShowDialog() == true;
    }

    public string? ShowOpenFileDialog(string filter, string title)
    {
        var dialog = new OpenFileDialog
        {
            Filter = filter,
            Title = title,
            CheckFileExists = true,
        };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public string? ShowSaveFileDialog(string filter, string title, string defaultFileName)
    {
        var dialog = new SaveFileDialog
        {
            Filter = filter,
            Title = title,
            FileName = defaultFileName,
            OverwritePrompt = true,
            AddExtension = true,
        };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public bool Confirm(string message, string title)
    {
        var owner = OwnerWindow;
        var result = owner is not null
            ? MessageBox.Show(owner, message, title, MessageBoxButton.YesNo, MessageBoxImage.Question)
            : MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
        return result == MessageBoxResult.Yes;
    }

    public void ShowMessage(string message, string title, bool isError = false)
    {
        var image = isError ? MessageBoxImage.Error : MessageBoxImage.Information;
        var owner = OwnerWindow;
        if (owner is not null)
            MessageBox.Show(owner, message, title, MessageBoxButton.OK, image);
        else
            MessageBox.Show(message, title, MessageBoxButton.OK, image);
    }
}
