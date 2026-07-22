using System.Windows;

namespace IpChanger.Views;

/// <summary>A használati útmutató ablak kódmögöttese. A tartalom lokalizált
/// (DynamicResource), így nyelvváltáskor is helyes.</summary>
public partial class ManualWindow : Window
{
    public ManualWindow()
    {
        InitializeComponent();
    }

    private void OnClose(object sender, RoutedEventArgs e) => Close();
}
