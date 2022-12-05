using System.Collections.Generic;
using System.Threading.Tasks;
using WeakAurasInterface.Core.Models;

namespace WeakAurasInterface.Core.Services.Interfaces;

/// <summary>
///     Services which provide access and storage management for application settings
/// </summary>
public interface ISettingsService
{
    public Settings Settings { get; }

    public string WeakAurasSavedPath { get; }

    public Task<bool> SaveAsync();

    public Task ReloadAsync();

    public bool IsValid();

    public bool WarcraftDirectoryValid();

    public IEnumerable<GameVersion> GetVersions();

    public IEnumerable<string> GetAccounts();
}