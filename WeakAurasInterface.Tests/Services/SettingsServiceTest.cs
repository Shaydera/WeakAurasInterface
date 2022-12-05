using WeakAurasInterface.Core.Models;
using WeakAurasInterface.Core.Services.Interfaces;
using WeakAurasInterface.WPF.Services;

namespace WeakAurasInterface.Tests.Services;

[TestFixture]
public class SettingsServiceTest
{
    [SetUp]
    public async Task SetUp()
    {
        _settingsService = await XmlSettingsService.BuildSettingsServiceAsync();
        Assert.That(_settingsService, Is.Not.Null);
        Assert.That(_settingsService.Settings, Is.Not.Null);
        _settingsService.Settings.WarcraftDirectory = TestFolder;
        _settingsService.Settings.GameVersion = TestVersion;
        _settingsService.Settings.AccountName = TestAccount;
    }

    private const string TestFolder = ".\\FolderTestEnvironment";
    private const GameVersion TestVersion = GameVersion.Retail;
    private const string TestAccount = "TestAccount#1";

    private const string FullSavePath =
        @".\FolderTestEnvironment\_retail_\WTF\Account\TestAccount#1\SavedVariables\WeakAuras.lua";

    private ISettingsService _settingsService = null!;

    [Test]
    public void IsValid()
    {
        Assert.That(_settingsService.IsValid, Is.True);
    }

    [Test]
    public void WeakAurasSavedPath()
    {
        Assert.That(_settingsService.WeakAurasSavedPath, Is.EqualTo(FullSavePath));
    }

    [Test]
    public async Task FileOperations()
    {
        await _settingsService.SaveAsync();
        Assert.That(File.Exists(".\\settings.xaml"), Is.True);
        await _settingsService.ReloadAsync();
        Assert.That(_settingsService.Settings.WarcraftDirectory, Is.EqualTo(TestFolder));
        Assert.That(_settingsService.Settings.GameVersion, Is.EqualTo(TestVersion));
        Assert.That(_settingsService.Settings.AccountName, Is.EqualTo(TestAccount));
    }
}