using System.Windows;

namespace IpChanger.Views;

/// <summary>
/// Az exportálás-választó ablak kódmögöttese. Az „Exportálás” gomb DialogResult=true
/// értékkel zárja be; a kijelölt preseteket a hívó a ViewModelből olvassa ki.
/// </summary>
public partial class ExportSelectionWindow : Window
{
    public ExportSelectionWindow()
    {
        InitializeComponent();
    }

    private void OnExport(object sender, RoutedEventArgs e) => DialogResult = true;

    private void OnCancel(object sender, RoutedEventArgs e) => DialogResult = false;
}
