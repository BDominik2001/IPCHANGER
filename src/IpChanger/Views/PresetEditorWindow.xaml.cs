using System.Windows;
using IpChanger.ViewModels;

namespace IpChanger.Views;

/// <summary>
/// A preset szerkesztő ablak kódmögöttese. Mentéskor validál a ViewModelen keresztül,
/// és csak érvényes adat esetén zárja be az ablakot DialogResult = true értékkel.
/// </summary>
public partial class PresetEditorWindow : Window
{
    public PresetEditorWindow()
    {
        InitializeComponent();
    }

    private void OnSave(object sender, RoutedEventArgs e)
    {
        if (DataContext is not PresetEditorViewModel vm)
            return;

        var error = vm.Validate();
        if (error is not null)
        {
            MessageBox.Show(this, error, vm.InvalidTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        DialogResult = true;
    }

    private void OnCancel(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
