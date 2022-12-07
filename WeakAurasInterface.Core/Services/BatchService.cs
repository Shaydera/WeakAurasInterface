using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using WeakAurasInterface.Core.Models;
using WeakAurasInterface.Core.Services.Interfaces;

namespace WeakAurasInterface.Core.Services;

public class BatchService : IBatchService
{
    private readonly IFileService _fileService;
    private readonly IWeakAurasService _weakAurasService;

    public BatchService(IFileService? fileService = null, IWeakAurasService? weakAurasService = null)
    {
        _fileService = fileService ?? Ioc.Default.GetRequiredService<IFileService>();
        _weakAurasService = weakAurasService ?? Ioc.Default.GetRequiredService<IWeakAurasService>();
    }

    public async Task<(bool isSuccess, int exportCount)> StartBatchExportAsync(string exportDir)
    {
        try
        {
            if (!Directory.Exists(exportDir))
                return (false, 0);
            
            var exportDirInfo = new DirectoryInfo(exportDir);
            exportDirInfo = exportDirInfo.CreateSubdirectory($"Export-{DateTime.Now:yyyyMMddTHHmmss}");
            List<WeakAuraDisplay> rootDisplays = (await _weakAurasService.GetAurasAsync(true)).ToList();

            if (!rootDisplays.Any())
                return (true, 0);

            IReadOnlyDictionary<string, string> exportDict =
                await _weakAurasService.ExportDisplaysAsStringAsync(rootDisplays);
            foreach (KeyValuePair<string, string> pair in exportDict)
            {
                var exportFile = $"{exportDirInfo.FullName}\\{_fileService.SanitizeFilename(pair.Key)}.txt";
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