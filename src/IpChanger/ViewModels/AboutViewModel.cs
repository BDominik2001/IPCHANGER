using System.Diagnostics;
using System.Reflection;

namespace IpChanger.ViewModels;

/// <summary>
/// A Névjegy ablak ViewModelje: alkalmazásnév, verzió (az assemblyből), készítő,
/// és a GitHub-oldal megnyitása.
/// </summary>
public sealed class AboutViewModel : ObservableObject
{
    public const string GitHubUrl = "https://github.com/BDominik2001/IPCHANGER";

    public AboutViewModel()
    {
        OpenGitHubCommand = new RelayCommand(OpenGitHub);
    }

    public string AppName => "IP Changer";

    public string Creator => "BDominik2001";

    /// <summary>Az alkalmazás verziója (major.minor.build) az assembly adataiból.</summary>
    public string Version
    {
        get
        {
            var v = Assembly.GetExecutingAssembly().GetName().Version;
            return v is null ? "1.0.0" : $"{v.Major}.{v.Minor}.{v.Build}";
        }
    }

    public string GitHub => GitHubUrl;

    public RelayCommand OpenGitHubCommand { get; }

    private void OpenGitHub()
    {
        try
        {
            Process.Start(new ProcessStartInfo(GitHubUrl) { UseShellExecute = true });
        }
        catch
        {
            // Ha nem nyitható meg a böngésző, csendben elnyeljük (nem kritikus).
        }
    }
}
