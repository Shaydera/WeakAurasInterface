using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Data;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using WeakAurasInterface.Core.Services.Interfaces;

namespace WeakAurasInterface.Core.ViewModels;

/// <summary>
///     ViewModel for a MDI Parent Window, which manages creation and display of all MDI Children
/// </summary>
public sealed class MainWindowViewModel : WorkspaceViewModel
{
    private readonly ISettingsService _settingsService;

    public MainWindowViewModel(string displayName, ISettingsService? settingsService = null) : base(displayName)
    {
        _settingsService = settingsService ?? Ioc.Default.GetRequiredService<ISettingsService>();
        Workspaces = new ObservableCollection<WorkspaceViewModel>();
        Workspaces.CollectionChanged += OnWorkspacesChanged;
        CollectionViewSource.GetDefaultView(Workspaces).CurrentChanged += CurrentWorkspaceChanged;
        StartupCommand = new RelayCommand(OnStartup);
        ChangeConfigCommand = new RelayCommand(ChangeConfiguration);
        ExportAurasCommand = new RelayCommand(ExportAuras);
    }

    public ObservableCollection<WorkspaceViewModel> Workspaces { get; }

    public WorkspaceViewModel CurrentWorkspace
    {
        get
        {
            if (CollectionViewSource.GetDefaultView(Workspaces)?.CurrentItem is WorkspaceViewModel viewModel)
                return viewModel;
            return this;
        }
    }

    public IRelayCommand StartupCommand { get; }

    public IRelayCommand ChangeConfigCommand { get; }

    public IRelayCommand ExportAurasCommand { get; }

    public override string Error => throw new NotImplementedException();

    public override string this[string columnName] => throw new NotImplementedException();

    private void CurrentWorkspaceChanged(object? sender, EventArgs e)
    {
        if (sender is ICollectionView { CurrentItem: ExportAurasViewModel exportVm })
            exportVm.ReloadCommand.Execute(null);
        OnPropertyChanged(nameof(CurrentWorkspace));
    }

    private void OnWorkspacesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is { Count: > 0 })
            foreach (WorkspaceViewModel workspace in e.NewItems)
                workspace.RequestClose += OnWorkspaceRequestClose;

        if (e.OldItems is { Count: > 0 })
            foreach (WorkspaceViewModel workspace in e.OldItems)
                workspace.RequestClose -= OnWorkspaceRequestClose;

        if (!Workspaces.Any())
            CloseCommand.Execute(null);
    }

    private void OnWorkspaceRequestClose(object? sender, EventArgs e)
    {
        if (sender is not WorkspaceViewModel workspace)
            throw new ArgumentNullException(
                $"{nameof(OnWorkspaceRequestClose)}: {nameof(sender)} is null or not a valid {nameof(WorkspaceViewModel)}.");

        if (workspace is ConfigurationViewModel && !_settingsService.IsValid())
            return;

        Workspaces.Remove(workspace);
    }

    private void ChangeConfiguration()
    {
        WorkspaceViewModel? configWorkspace =
            Workspaces.SingleOrDefault(workspace => workspace is ConfigurationViewModel);
        if (configWorkspace == null)
        {
            configWorkspace = new ConfigurationViewModel("Update Configuration");
            Workspaces.Add(configWorkspace);
        }

        SetActiveWorkspace(configWorkspace);
    }

    private void ExportAuras()
    {
        WorkspaceViewModel? exportWorkspace =
            Workspaces.SingleOrDefault(workspace => workspace is ExportAurasViewModel);
        if (exportWorkspace == null)
        {
            exportWorkspace = new ExportAurasViewModel("Export Auras");
            Workspaces.Add(exportWorkspace);
        }

        SetActiveWorkspace(exportWorkspace);
    }

    private void SetActiveWorkspace(WorkspaceViewModel workspace)
    {
        Debug.Assert(Workspaces.Contains(workspace));
        ICollectionView workspaceView = CollectionViewSource.GetDefaultView(Workspaces);
        workspaceView?.MoveCurrentTo(workspace);
    }

    private void OnStartup()
    {
        ExportAuras();
        if (!_settingsService.IsValid())
            ChangeConfiguration();
    }
}