using System.Threading.Tasks;

namespace WeakAurasInterface.Core.Services.Interfaces;

/// <summary>
///     Services which provides file and folder operations
/// </summary>
public interface IFileService
{
    public string BrowseDirectory(string initialDirectory);

    public Task<bool> CreateFileWithContentAsync(string fileName, string fileContent);

    public string SanitizeFilename(string fileName);

    public void OpenDirectory(string directory);

    public string BuildFilename(string directory, string filename, string extension);
}