using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WeakAurasInterface.Core.Models;
using WeakAurasInterface.Core.Services.Interfaces;

namespace WeakAurasInterface.Core.Services;

public abstract class SettingsServiceBase : ISettingsService
{
    protected SettingsServiceBase(Settings settings)
    {
        Settings = settings;
    }

    public abstract Task<bool> SaveAsync();
    public abstract Task ReloadAsync();

    public Settings Settings { get; protected set; }

    public string WeakAurasSavedPath
    {
        get
        {
            if (!IsValid())
                return string.Empty;
            string filePath =
                $"{Settings.WarcraftDirectory}\\{Settings.GameVersion?.ToDirectoryName()}\\WTF\\Account\\{Settings.AccountName}\\SavedVariables\\WeakAuras.lua";
            return filePath;
        }
    }

    public bool IsValid()
    {
        if (!WarcraftDirectoryValid())
            return false;

        if (!Settings.GameVersion.HasValue)
            return false;

        if (string.IsNullOrWhiteSpace(Settings.AccountName))
            return false;

        return true;
    }

    public bool WarcraftDirectoryValid()
    {
        if (!Directory.Exists(Settings.WarcraftDirectory))
            return false;

        var result = false;
        var directoryInfo = new DirectoryInfo(Settings.WarcraftDirectory);
        foreach (DirectoryInfo subDir in directoryInfo.GetDirectories())
        {
            if (GameVersionExtensions.DirectoryNameDict.ContainsValue(subDir.Name))
            {
                result = true;
                break;
            }

            if (result)
                break;
        }

        return result;
    }

    public IEnumerable<GameVersion> GetVersions()
    {
        if (!WarcraftDirectoryValid())
            return Enumerable.Empty<GameVersion>();

        Dictionary<GameVersion, string> versionDirDict = GameVersionExtensions.DirectoryNameDict;
        var dirInfo = new DirectoryInfo(Settings.WarcraftDirectory);
        DirectoryInfo[] subDirInfos = dirInfo.GetDirectories();
        IList<GameVersion> result =
            (from pair in versionDirDict
                where subDirInfos.Any(info => info.Name.Equals(pair.Value))
                select pair.Key).ToList();
        return result;
    }

    public IEnumerable<string> GetAccounts()
    {
        if (!WarcraftDirectoryValid() || !Settings.GameVersion.HasValue)
            return Enumerable.Empty<string>();

        var result = new List<string>();
        var accountsDir =
            $"{Settings.WarcraftDirectory}\\{Settings.GameVersion.Value.ToDirectoryName()}\\WTF\\Account\\";
        if (Directory.Exists(accountsDir))
            result.AddRange(from directory in Directory.EnumerateDirectories(accountsDir)
                let savedDirInfo = new DirectoryInfo($"{directory}\\SavedVariables")
                where savedDirInfo.Exists &&
                      savedDirInfo.EnumerateFiles().Any(file => file.Name.Equals("WeakAuras.lua"))
                select new DirectoryInfo(directory).Name);

        return result;
    }
}