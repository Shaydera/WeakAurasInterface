using System;
using System.Collections.Generic;
using System.Linq;

namespace WeakAurasInterface.Core.Models;

/// <summary>
///     Enum containing all available/supported World of Warcraft Game Versions.
/// </summary>
public enum GameVersion
{
    Retail = 0,
    Classic = 1,
    VanillaEra = 2,
    Beta = 3,
    Ptr = 4
}

public static class GameVersionExtensions
{
    public static Dictionary<GameVersion, string> DirectoryNameDict
    {
        get
        {
            return Enum.GetValues<GameVersion>()
                .ToDictionary(gameVersion => gameVersion, gameVersion => gameVersion.ToDirectoryName());
        }
    }

    public static string ToDirectoryName(this GameVersion enumValue)
    {
        return enumValue switch
        {
            GameVersion.Retail => "_retail_",
            GameVersion.Classic => "_classic_",
            GameVersion.VanillaEra => "_era_",
            GameVersion.Beta => "_beta_",
            GameVersion.Ptr => "_ptr_",
            _ => throw new ArgumentOutOfRangeException(nameof(enumValue), enumValue, null)
        };
    }
}