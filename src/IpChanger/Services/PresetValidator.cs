using System.Net;
using System.Net.Sockets;
using IpChanger.Models;

namespace IpChanger.Services;

/// <summary>
/// Presetek szintaktikai ellenőrzése az alkalmazás előtt (IPv4 formátumok, kötelező mezők).
/// </summary>
public static class PresetValidator
{
    /// <summary>Igaz, ha a szöveg egy érvényes IPv4 cím.</summary>
    public static bool IsValidIpv4(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        // Az IPAddress.TryParse elfogad olyan alakokat is, mint "1" vagy "1.2",
        // ezért kikötjük a négy oktettes, pontokkal tagolt formát is.
        var parts = value.Split('.');
        if (parts.Length != 4)
            return false;

        return IPAddress.TryParse(value, out var ip) && ip.AddressFamily == AddressFamily.InterNetwork;
    }

    /// <summary>
    /// Ellenőrzi a presetet. Ha nem érvényes, egy hibaüzenetet ad vissza; siker esetén null.
    /// </summary>
    public static string? Validate(IpPreset preset)
    {
        if (string.IsNullOrWhiteSpace(preset.Name))
            return "A preset neve nem lehet üres.";

        if (preset.UseDhcp)
            return null; // DHCP esetén nincs statikus mező, amit ellenőrizni kellene.

        if (!IsValidIpv4(preset.IpAddress))
            return "Az IP cím nem érvényes IPv4 formátumú.";

        if (!IsValidIpv4(preset.SubnetMask))
            return "Az alhálózati maszk nem érvényes IPv4 formátumú.";

        if (!string.IsNullOrWhiteSpace(preset.Gateway) && !IsValidIpv4(preset.Gateway))
            return "Az átjáró (gateway) nem érvényes IPv4 formátumú.";

        if (!preset.DnsFromDhcp)
        {
            if (!string.IsNullOrWhiteSpace(preset.PreferredDns) && !IsValidIpv4(preset.PreferredDns))
                return "Az elsődleges DNS nem érvényes IPv4 formátumú.";

            if (!string.IsNullOrWhiteSpace(preset.AlternateDns) && !IsValidIpv4(preset.AlternateDns))
                return "A másodlagos DNS nem érvényes IPv4 formátumú.";
        }

        return null;
    }
}
