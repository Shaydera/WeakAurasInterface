using System.Collections.Generic;
using System.Threading.Tasks;
using WeakAurasInterface.Core.Models;

namespace WeakAurasInterface.Core.Services.Interfaces;

/// <summary>
///     Services for interaction with WeakAuras
/// </summary>
public interface IWeakAurasService
{
    public Task<IEnumerable<WeakAuraDisplay>> GetAurasAsync(bool rootNodesOnly = false);

    public Task<IReadOnlyDictionary<string, string>> ExportDisplaysAsStringAsync(IEnumerable<WeakAuraDisplay> displays);
}