namespace WeakAurasInterface.Core.Models;

/// <summary>
///     Application Settings Record
/// </summary>
public record Settings
{
    public string AccountName = string.Empty;
    public string ExportDirectory = string.Empty;
    public GameVersion? GameVersion;
    public string WarcraftDirectory = string.Empty;
}