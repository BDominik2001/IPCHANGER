namespace IpChanger.Models;

/// <summary>
/// Egy művelet (pl. IP beállítás) eredménye, sikerjelzéssel és üzenettel.
/// </summary>
public sealed class OperationResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;

    public static OperationResult Ok(string message) => new() { Success = true, Message = message };
    public static OperationResult Fail(string message) => new() { Success = false, Message = message };
}
