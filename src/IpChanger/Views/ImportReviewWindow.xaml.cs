using System.Windows;

namespace IpChanger.Views;

/// <summary>
/// Az importálás-áttekintő ablak kódmögöttese. Az „Importálás” gomb DialogResult=true
/// értékkel zárja be; a választott műveleteket a hívó a ViewModelből olvassa ki.
/// </summary>
public partial class ImportReviewWindow : Window
{
    public ImportReviewWindow()
    {
        InitializeComponent();
    }

    private void OnImport(object sender, RoutedEventArgs e) => DialogResult = true;

    private void OnCancel(object sender, RoutedEventArgs e) => DialogResult = false;
}
