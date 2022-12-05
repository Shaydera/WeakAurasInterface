using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NLua;
using WeakAurasInterface.Core.Lua;
using WeakAurasInterface.Core.Models;
using WeakAurasInterface.Core.Services.Interfaces;

namespace WeakAurasInterface.Core.Services;

/// <summary>
///     Lua based implementation of IWeakAurasService
///     TODO: Serialize WeakAuras.lua (Save File) and fill WeakAurasSaved Table from C# 
///     TODO: so we do not need to execute an untrusted lua file.
///     TODO: Alternatively sandbox the Lua Environment.
/// </summary>
public class WeakAurasLuaService : IWeakAurasService
{
    private readonly ISettingsService _settingsService;

    public WeakAurasLuaService(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public async Task<IEnumerable<WeakAuraDisplay>> GetAurasAsync(bool rootNodesOnly = false)
    {
        if (!File.Exists(_settingsService.WeakAurasSavedPath))
            return Enumerable.Empty<WeakAuraDisplay>();

        try
        {
            IEnumerable<WeakAuraDisplay> result = await Task.Run(() =>
            {
                using var luaEnvironment = new NLua.Lua();
                //TODO: See class XML DOC
                luaEnvironment.DoFile(_settingsService.WeakAurasSavedPath);
                using LuaTable displayTable = luaEnvironment.GetTable("WeakAurasSaved.displays");
                IReadOnlyDictionary<object, object> displayDict = luaEnvironment.GetTableDict(displayTable);
                IEnumerable<WeakAuraDisplay> displayList = BuildDisplayListFromDict(displayDict);
                if (rootNodesOnly)
                    return displayList.Where(display => display.Parent == null); //Root nodes have no parent.
                return displayList;
            });
            return result;
        }
        finally
        {
            //Force GC to ensure the managed memory usage of the Lua State is getting freed already.
            GC.Collect(2);
        }
    }

    public async Task<IReadOnlyDictionary<string, string>> ExportDisplaysAsStringAsync(IEnumerable<WeakAuraDisplay> displays)
    {
        if (!_settingsService.IsValid())
            return new Dictionary<string, string>();

        try
        {
            Dictionary<string, string> results = await Task.Run(() =>
            {
                var exportedDict = new Dictionary<string, string>();
                using var luaEnvironment = new NLua.Lua();
                luaEnvironment.DoString(EmbeddedLua.MathLibLua);
                luaEnvironment["LibDeflate"] = luaEnvironment.DoString(EmbeddedLua.LibDeflateLua)[0];
                luaEnvironment["LibSerialize"] = luaEnvironment.DoString(EmbeddedLua.LibSerializeLua)[0];
                //TODO: See class XML DOC
                luaEnvironment.DoFile(_settingsService.WeakAurasSavedPath);
                var exportFunction = luaEnvironment.DoString(EmbeddedLua.ExportDisplayLua)[0] as LuaFunction;
                foreach (WeakAuraDisplay weakAuraDisplay in displays)
                {
                    object[]? exportResult = exportFunction?.Call(weakAuraDisplay.Id);
                    if (exportResult is { Length: > 0 } && exportResult[0] is string exportString &&
                        exportString.StartsWith("!WA"))
                        exportedDict.Add(weakAuraDisplay.Id, exportString);
                }

                return exportedDict;
            });

            return results;
        }
        finally
        {
            //Force GC to ensure the managed memory usage of the Lua State is getting freed already.
            GC.Collect(2);
        }
    }

    private static IEnumerable<WeakAuraDisplay> BuildDisplayListFromDict(
        IReadOnlyDictionary<object, object> displayDict)
    {
        var result = new Dictionary<string, WeakAuraDisplay>();
        foreach (LuaTable entry in displayDict.Values)
        {
            if (entry["id"] is not string id || string.IsNullOrWhiteSpace(id))
            {
                Debug.Fail(nameof(BuildDisplayListFromDict));
                continue;
            }

            var display = new WeakAuraDisplay(id);
            result.Add(display.Id, display);
        }

        //Populate Parent links
        foreach (LuaTable entry in displayDict.Values)
        {
            if (entry["id"] is not string id ||
                entry["parent"] is not string parentId ||
                string.IsNullOrWhiteSpace(parentId) ||
                !result.ContainsKey(id) ||
                !result.ContainsKey(parentId))
                continue;
            result[id].Parent = result[parentId];
        }

        return result.Values;
    }
}