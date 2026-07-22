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
    private ILocalizationService? _loc;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Nem várt kivételek barátságos kezelése összeomlás helyett.
        DispatcherUnhandledException += OnUnhandledException;

        var settingsStore = new SettingsStore();
        var settings = settingsStore.Load();

        var localization = new LocalizationService(settingsStore);
        localization.Apply(settings.Language);
        _loc = localization;

        var themeService = new ThemeService(settingsStore);
        themeService.Apply(settings.Theme);

        var store = new PresetStore();
        var network = new NetworkAdapterService(localization);
        var dialogs = new DialogService();

        var viewModel = new MainViewModel(store, network, dialogs, themeService, localization);
        var window = new MainWindow { DataContext = viewModel };
        MainWindow = window;
        window.Show();
    }

    private void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        var title = _loc?.Get("L.App.ErrorTitle") ?? "IP Changer";
        var message = _loc is not null
            ? _loc.Format("L.App.UnhandledError", e.Exception.Message)
            : $"Unexpected error:\n\n{e.Exception.Message}";
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }
}
