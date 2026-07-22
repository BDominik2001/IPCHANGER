using System.Windows;

namespace IpChanger.Views;

/// <summary>A Névjegy ablak kódmögöttese.</summary>
public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();
    }

    private void OnClose(object sender, RoutedEventArgs e) => Close();
}
