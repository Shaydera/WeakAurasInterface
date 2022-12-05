using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WeakAurasInterface.Core.Models;
using WeakAurasInterface.Core.Services.Interfaces;

namespace WeakAurasInterface.Core.Services;

public class BatchService : IBatchService
{
    private readonly IFileService _fileService;
    private readonly IWeakAurasService _weakAurasService;

    public BatchService(IFileService fileService, IWeakAurasService weakAurasService)
    {
        _fileService = fileService;
        _weakAurasService = weakAurasService;
    }

    public async Task<(bool isSuccess, int exportCount)> StartBatchExportAsync(string exportDir)
    {
        try
        {
            if (!Directory.Exists(exportDir))
                return (false, 0);

            List<WeakAuraDisplay> rootDisplays = (await _weakAurasService.GetAurasAsync(true)).ToList();

            if (!rootDisplays.Any())
                return (false, 0);

            IReadOnlyDictionary<string, string> exportDict =
                await _weakAurasService.ExportDisplaysAsStringAsync(rootDisplays);
            foreach (KeyValuePair<string, string> pair in exportDict)
            {
                var exportFile = $"{exportDir}\\{_fileService.SanitizeFilename(pair.Key)}.txt";
                await _fileService.CreateFileWithContentAsync(exportFile, pair.Value);
            }

            return (true, exportDict.Count);
        }
        catch (Exception)
        {
            return (false, 0);
        }
    }
}