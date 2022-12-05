using System.Threading.Tasks;

namespace WeakAurasInterface.Core.Services.Interfaces;

public interface IBatchService
{
    public Task<(bool isSuccess, int exportCount)> StartBatchExportAsync(string exportDir);
}