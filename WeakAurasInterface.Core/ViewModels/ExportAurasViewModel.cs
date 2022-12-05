using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using WeakAurasInterface.Core.Models;
using WeakAurasInterface.Core.Services.Interfaces;

namespace WeakAurasInterface.Core.ViewModels;

/// <summary>
///     ViewModel for a View allowing the export of WeakAuras
/// </summary>
public sealed class ExportAurasViewModel : WorkspaceViewModel
{
    private readonly IFileService _fileService;
    private readonly ISettingsService _settingsService;
    private readonly IWeakAurasService _weakAurasService;
    private ObservableCollection<WeakAuraDisplay> _loadedDisplays = new(Enumerable.Empty<WeakAuraDisplay>());
    private ObservableCollection<WeakAuraDisplay> _selectedDisplays = new(Enumerable.Empty<WeakAuraDisplay>());

    public ExportAurasViewModel(string displayName) : base(displayName)
    {
        _fileService = Ioc.Default.GetRequiredService<IFileService>();
        _settingsService = Ioc.Default.GetRequiredService<ISettingsService>();
        _weakAurasService = Ioc.Default.GetRequiredService<IWeakAurasService>();
        ReloadCommand = new AsyncRelayCommand(OnReloadAsync);
        ExportSelectedDisplaysCommand = new AsyncRelayCommand(ExportSelectedDisplays);
        AddToSelectionCommand = new RelayCommand<WeakAuraDisplay>(AddDisplayToSelection);
        RemoveFromSelectionCommand = new RelayCommand<WeakAuraDisplay>(RemoveDisplayFromSelection);
        ClearSelectionCommand = new RelayCommand(ClearSelection);
        SelectAllDisplaysCommand = new RelayCommand(SelectAllDisplays);
    }

    public ObservableCollection<WeakAuraDisplay> SelectedDisplays
    {
        get => _selectedDisplays;
        set => SetProperty(ref _selectedDisplays, value);
    }

    public ObservableCollection<WeakAuraDisplay> LoadedDisplays
    {
        get => _loadedDisplays;
        set => SetProperty(ref _loadedDisplays, value);
    }

    public IAsyncRelayCommand ReloadCommand { get; }

    public IAsyncRelayCommand ExportSelectedDisplaysCommand { get; }

    public IRelayCommand AddToSelectionCommand { get; }

    public IRelayCommand RemoveFromSelectionCommand { get; }

    public IRelayCommand ClearSelectionCommand { get; }

    public IRelayCommand SelectAllDisplaysCommand { get; }

    public override string Error => throw new NotImplementedException();

    public override string this[string columnName]
    {
        get
        {
            var result = string.Empty;
            switch (columnName)
            {
                case nameof(LoadedDisplays):
                    if (!LoadedDisplays.Any())
                        result = "Aura collection is empty.";
                    break;
                default:
                    throw new NotImplementedException();
            }

            return result;
        }
    }

    public async Task OnReloadAsync()
    {
        SelectedDisplays.Clear();
        IEnumerable<WeakAuraDisplay> displays = await _weakAurasService.GetAurasAsync(true);
        LoadedDisplays = new ObservableCollection<WeakAuraDisplay>(displays.OrderBy(display => display.Id));
    }

    public void AddDisplayToSelection(WeakAuraDisplay? targetDisplay)
    {
        if (targetDisplay != null && !SelectedDisplays.Contains(targetDisplay))
            SelectedDisplays.Add(targetDisplay);
    }

    public void RemoveDisplayFromSelection(WeakAuraDisplay? targetDisplay)
    {
        if (targetDisplay != null)
            SelectedDisplays.Remove(targetDisplay);
    }

    public void ClearSelection()
    {
        SelectedDisplays.Clear();
    }

    public void SelectAllDisplays()
    {
        foreach (WeakAuraDisplay display in LoadedDisplays)
            if (!SelectedDisplays.Contains(display))
                SelectedDisplays.Add(display);
    }

    public async Task ExportSelectedDisplays()
    {
        if (!SelectedDisplays.Any())
            return;

        IReadOnlyDictionary<string, string> exportDict =
            await _weakAurasService.ExportDisplaysAsStringAsync(SelectedDisplays);

        string exportDir = _fileService.BrowseDirectory(_settingsService.Settings.ExportDirectory);
        if (!Directory.Exists(exportDir))
            return;

        var taskBag = new ConcurrentBag<Task>();
        foreach (KeyValuePair<string, string> pair in exportDict)
        {
            string fileName = _fileService.SanitizeFilename(pair.Key);
            var fullFilename = $"{exportDir}\\{fileName}.txt";
            taskBag.Add(_fileService.CreateFileWithContentAsync(fullFilename, pair.Value));
        }

        await Task.WhenAll(taskBag);
        SelectedDisplays.Clear();
        _fileService.OpenDirectory(exportDir);
    }
}