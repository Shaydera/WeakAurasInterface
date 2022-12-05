using WeakAurasInterface.Core.Models;
using WeakAurasInterface.Core.Services;
using WeakAurasInterface.Core.Services.Interfaces;
using WeakAurasInterface.WPF.Services;

namespace WeakAurasInterface.Tests.Services;

[TestFixture]
public class WeakAurasLuaServiceTest
{
    private WeakAurasLuaService _luaService;
    private ISettingsService _settingsService;

    [SetUp]
    public async Task SetUp()
    {
        _settingsService = await XmlSettingsService.BuildSettingsServiceAsync();
        _settingsService.Settings.WarcraftDirectory = ".\\FolderTestEnvironment";
        _settingsService.Settings.GameVersion = GameVersion.Retail;
        _settingsService.Settings.AccountName = "TestAccount#1";
        _luaService = new WeakAurasLuaService(_settingsService);
    }

    [Test]
    public async Task GetAurasAsync()
    {
        IEnumerable<WeakAuraDisplay> auras = (await _luaService.GetAurasAsync()).ToList();
        Assert.That(auras, Is.Not.Null.Or.Empty);
        Assert.That(auras.Count(), Is.EqualTo(1));
        Assert.That(auras.Single(display => display.Id == "TestAuraOne"), Is.Not.Null);
    }

    [Test]
    public async Task ExportDisplaysAsStringAsync()
    {
        var exportAura = new WeakAuraDisplay("TestAuraOne");
        var exportList = new List<WeakAuraDisplay> { exportAura };
        IReadOnlyDictionary<string, string> result = await _luaService.ExportDisplaysAsStringAsync(exportList);
        Assert.That(result, Is.Not.Null.Or.Empty);
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result, Does.ContainKey(exportAura.Id));
        Assert.That(result[exportAura.Id], Does.StartWith("!WA"));
    }
}