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
///     ViewModel allowing for a view to manage application settings.
/// </summary>
public sealed class ConfigurationViewModel : WorkspaceViewModel
{
    private readonly IFileService _fileService;
    private readonly ISettingsService _settingsService;

    private bool _inputEnabled = true;


    private string _lastError = string.Empty;

    public ConfigurationViewModel(string displayName, IFileService? fileService = null,
        ISettingsService? settingsService = null) : base(displayName)
    {
        _settingsService = settingsService ?? Ioc.Default.GetRequiredService<ISettingsService>();
        _fileService = fileService ?? Ioc.Default.GetRequiredService<IFileService>();
        SaveCommand = new AsyncRelayCommand(SaveConfiguration);
        BrowseWarcraftDirectoryCommand = new RelayCommand(BrowseWarcraftDirectory);
    }

    public bool InputEnabled
    {
        get => _inputEnabled;
        private set
        {
            SetProperty(ref _inputEnabled, value);
            OnPropertyChanged(nameof(CanSave));
        }
    }

    public override string Error => _lastError;

    public string WarcraftDirectory
    {
        get => _settingsService.Settings.WarcraftDirectory;
        set
        {
            value = value.Trim();
            if (string.IsNullOrEmpty(value) || !Directory.Exists(value))
                return;

            SelectedAccount = string.Empty;
            SelectedVersion = null;
            SetProperty(_settingsService.Settings.WarcraftDirectory, value, _settingsService,
                (service, directory) => service.Settings.WarcraftDirectory = directory);
            OnPropertyChanged(nameof(AvailableVersions));
        }
    }

    public GameVersion? SelectedVersion
    {
        get => _settingsService.Settings.GameVersion;
        set
        {
            SetProperty(_settingsService.Settings.GameVersion, value, _settingsService,
                (service, version) => service.Settings.GameVersion = version);
            OnPropertyChanged(nameof(AvailableAccounts));
        }
    }

    public ObservableCollection<GameVersion> AvailableVersions
    {
        get
        {
            if (!_settingsService.WarcraftDirectoryValid())
                return new ObservableCollection<GameVersion>();

            var versions = new ObservableCollection<GameVersion>(_settingsService.GetVersions());
            SelectedVersion ??= versions.FirstOrDefault();
            return versions;
        }
    }

    public string SelectedAccount
    {
        get => _settingsService.Settings.AccountName;
        set
        {
            SetProperty(_settingsService.Settings.AccountName, value, _settingsService,
                (service, account) => service.Settings.AccountName = account);
            OnPropertyChanged(nameof(CanSave));
        }
    }

    public ObservableCollection<string> AvailableAccounts
    {
        get
        {
            if (!_settingsService.WarcraftDirectoryValid() || SelectedVersion == null)
                return new ObservableCollection<string>();

            var accounts = new ObservableCollection<string>(_settingsService.GetAccounts());
            if (string.IsNullOrWhiteSpace(SelectedAccount))
                SelectedAccount = accounts.FirstOrDefault() ?? string.Empty;
            return accounts;
        }
    }

    public bool CanSave => InputEnabled && _settingsService.IsValid();

    public IAsyncRelayCommand SaveCommand { get; }

    public IRelayCommand BrowseWarcraftDirectoryCommand { get; }

    public override string this[string columnName]
    {
        get
        {
            var result = string.Empty;

            switch (columnName)
            {
                case nameof(WarcraftDirectory):
                {
                    if (!_settingsService.WarcraftDirectoryValid())
                        result = "Invalid directory";
                    break;
                }
                case nameof(SelectedVersion):
                {
                    if (!SelectedVersion.HasValue)
                        result = "Invalid game version";
                    break;
                }
                case nameof(SelectedAccount):
                {
                    if (string.IsNullOrWhiteSpace(SelectedAccount))
                        result = "Invalid account name";
                    break;
                }
            }

            SetProperty(ref _lastError, result, nameof(Error));
            return result;
        }
    }

    private void BrowseWarcraftDirectory()
    {
        WarcraftDirectory = _fileService.BrowseDirectory(WarcraftDirectory);
    }

    private async Task SaveConfiguration()
    {
        if (!CanSave)
            return;
        InputEnabled = false;
        await _settingsService.SaveAsync();
        CloseCommand.Execute(null);
    }
}