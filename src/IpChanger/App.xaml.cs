using System.Windows;
using System.Windows.Threading;
using IpChanger.Services;
using IpChanger.ViewModels;
using IpChanger.Views;

namespace IpChanger;

/// <summary>
/// Az alkalmazás belépési pontja. Összeállítja a függőségeket és megnyitja a főablakot.
/// A rendszergazdai jogot az app.manifest biztosítja (requireAdministrator).
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Nem várt kivételek barátságos kezelése összeomlás helyett.
        DispatcherUnhandledException += OnUnhandledException;

        var store = new PresetStore();
        var network = new NetworkAdapterService();
        var dialogs = new DialogService();

        var viewModel = new MainViewModel(store, network, dialogs);
        var window = new MainWindow { DataContext = viewModel };
        MainWindow = window;
        window.Show();
    }

    private void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show(
            $"Váratlan hiba történt:\n\n{e.Exception.Message}",
            "IP Changer — hiba",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
        e.Handled = true;
    }
}
