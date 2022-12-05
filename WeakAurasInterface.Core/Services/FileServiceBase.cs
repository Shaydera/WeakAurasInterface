using System;
using System.IO;
using System.Threading.Tasks;
using WeakAurasInterface.Core.Services.Interfaces;

namespace WeakAurasInterface.Core.Services;

public abstract class FileServiceBase : IFileService
{
    public abstract string BrowseDirectory(string initialDirectory);

    public async Task<bool> CreateFileWithContentAsync(string fileName, string fileContent)
    {
        await File.WriteAllTextAsync(fileName, fileContent);
        return true;
    }

    public string SanitizeFilename(string fileName)
    {
        fileName = fileName.Trim();
        char[] invalidChars = Path.GetInvalidFileNameChars();
        return string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
    }

    public abstract void OpenDirectory(string directory);
}