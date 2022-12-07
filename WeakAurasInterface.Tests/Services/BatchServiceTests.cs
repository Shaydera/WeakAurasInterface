using WeakAurasInterface.Core.Models;
using WeakAurasInterface.Core.Services;
using WeakAurasInterface.Core.Services.Interfaces;
using WeakAurasInterface.WPF.Services;

namespace WeakAurasInterface.Tests.Services;

[TestFixture]
public class BatchServiceTests
{
    [SetUp]
    public async Task SetUp()
    {
        ISettingsService settingsService = await XmlSettingsService.BuildSettingsServiceAsync();
        settingsService.Settings.WarcraftDirectory = ".\\FolderTestEnvironment";
        settingsService.Settings.GameVersion = GameVersion.Retail;
        settingsService.Settings.AccountName = "TestAccount#1";
        var luaService = new WeakAurasLuaService(settingsService);
        _batchService = new BatchService(new Win32FileService(), luaService);

        if (Directory.Exists(ExportDir)) Directory.Delete(ExportDir, true);
        Directory.CreateDirectory(ExportDir);
    }

    private BatchService _batchService = null!;
    private const string ExportDir = ".\\FolderTestEnvironment\\BatchTest\\";

    [Test]
    public async Task StartBatchExportAsyncTest()
    {
        (bool isSuccess, int exportCount) = await _batchService.StartBatchExportAsync(ExportDir);
        Assert.That(isSuccess, Is.True);
        Assert.That(exportCount, Is.GreaterThan(0));
        Assert.That(Directory.GetFiles(ExportDir,string.Empty, SearchOption.AllDirectories).Length, Is.EqualTo(exportCount));
    }
}