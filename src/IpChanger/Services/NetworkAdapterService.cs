using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using IpChanger.Models;

namespace IpChanger.Services;

/// <summary>
/// Hálózati adapterek lekérdezése és IP-konfiguráció alkalmazása.
/// A tényleges módosítást a Windows beépített "netsh" eszközével végzi,
/// amelyhez rendszergazdai jog szükséges (ezt a manifest biztosítja).
/// </summary>
public sealed class NetworkAdapterService
{
    private readonly ILocalizationService _loc;

    public NetworkAdapterService(ILocalizationService loc)
    {
        _loc = loc;
    }

    /// <summary>Elérhető, fizikai/virtuális hálózati adapterek listája (loopback és tunnel nélkül).</summary>
    public IReadOnlyList<NetworkAdapterInfo> GetAdapters()
    {
        var result = new List<NetworkAdapterInfo>();

        foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (nic.NetworkInterfaceType is NetworkInterfaceType.Loopback or NetworkInterfaceType.Tunnel)
                continue;

            string? ipv4 = null;
            try
            {
                foreach (var ua in nic.GetIPProperties().UnicastAddresses)
                {
                    if (ua.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        ipv4 = ua.Address.ToString();
                        break;
                    }
                }
            }
            catch
            {
                // Egyes adaptereknél a lekérdezés hibázhat; ilyenkor egyszerűen nincs IP infó.
            }

            result.Add(new NetworkAdapterInfo
            {
                Name = nic.Name,
                Description = nic.Description,
                CurrentIpv4 = ipv4,
                IsUp = nic.OperationalStatus == OperationalStatus.Up,
            });
        }

        return result
            .OrderByDescending(a => a.IsUp)
            .ThenBy(a => a.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    /// <summary>
    /// A megadott presetet alkalmazza az adott adapterre. Több netsh parancsot futtat
    /// (IP, majd DNS). Az első hibánál megáll és a hibaüzenetet adja vissza.
    /// </summary>
    public OperationResult ApplyPreset(string adapterName, IpPreset preset)
    {
        if (string.IsNullOrWhiteSpace(adapterName))
            return OperationResult.Fail(_loc.Get("L.Val.NoAdapter"));

        var validationError = PresetValidator.Validate(preset, _loc);
        if (validationError is not null)
            return OperationResult.Fail(validationError);

        try
        {
            return preset.UseDhcp
                ? ApplyDhcp(adapterName, preset)
                : ApplyStatic(adapterName, preset);
        }
        catch (Exception ex)
        {
            return OperationResult.Fail(_loc.Format("L.Net.Unexpected", ex.Message));
        }
    }

    private OperationResult ApplyDhcp(string adapterName, IpPreset preset)
    {
        // IP cím DHCP-ről
        var ipResult = RunNetsh($"interface ipv4 set address name=\"{adapterName}\" source=dhcp");
        if (!ipResult.Success)
            return ipResult;

        // DNS DHCP-ről
        var dnsResult = RunNetsh($"interface ipv4 set dnsservers name=\"{adapterName}\" source=dhcp");
        if (!dnsResult.Success)
            return dnsResult;

        return OperationResult.Ok(_loc.Format("L.Net.DhcpSet", adapterName));
    }

    private OperationResult ApplyStatic(string adapterName, IpPreset preset)
    {
        // Statikus IP + maszk (+ opcionális átjáró)
        var addressCommand = new StringBuilder();
        addressCommand.Append($"interface ipv4 set address name=\"{adapterName}\" static {preset.IpAddress} {preset.SubnetMask}");
        if (!string.IsNullOrWhiteSpace(preset.Gateway))
            addressCommand.Append($" {preset.Gateway}");

        var ipResult = RunNetsh(addressCommand.ToString());
        if (!ipResult.Success)
            return ipResult;

        // DNS beállítása
        if (preset.DnsFromDhcp)
        {
            var dnsResult = RunNetsh($"interface ipv4 set dnsservers name=\"{adapterName}\" source=dhcp");
            if (!dnsResult.Success)
                return dnsResult;
        }
        else if (!string.IsNullOrWhiteSpace(preset.PreferredDns))
        {
            var primary = RunNetsh(
                $"interface ipv4 set dnsservers name=\"{adapterName}\" static {preset.PreferredDns} primary");
            if (!primary.Success)
                return primary;

            if (!string.IsNullOrWhiteSpace(preset.AlternateDns))
            {
                var secondary = RunNetsh(
                    $"interface ipv4 add dnsservers name=\"{adapterName}\" address={preset.AlternateDns} index=2");
                if (!secondary.Success)
                    return secondary;
            }
        }

        return OperationResult.Ok(
            _loc.Format("L.Net.StaticSet", adapterName, preset.IpAddress, preset.SubnetMask));
    }

    /// <summary>Egy netsh parancs futtatása ablak nélkül, a kimenet visszaadásával.</summary>
    private OperationResult RunNetsh(string arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "netsh",
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        using var process = Process.Start(startInfo);
        if (process is null)
            return OperationResult.Fail(_loc.Get("L.Net.NoProcess"));

        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode == 0)
            return OperationResult.Ok(string.IsNullOrWhiteSpace(stdout) ? "OK" : stdout.Trim());

        var message = !string.IsNullOrWhiteSpace(stderr) ? stderr.Trim()
            : !string.IsNullOrWhiteSpace(stdout) ? stdout.Trim()
            : _loc.Format("L.Net.ExitCode", process.ExitCode);
        return OperationResult.Fail(message);
    }
}
