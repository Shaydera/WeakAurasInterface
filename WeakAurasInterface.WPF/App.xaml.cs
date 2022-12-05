using System;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using WeakAurasInterface.Core.Services;
using WeakAurasInterface.Core.Services.Interfaces;
using WeakAurasInterface.Core.ViewModels;
using WeakAurasInterface.WPF.Services;
using WeakAurasInterface.WPF.Views;

namespace WeakAurasInterface.WPF;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App
{
    protected MainWindow? MdiParentWindow { get; private set; }

    protected async Task SetupDependencies()
    {
        var settingsService = await XmlSettingsService.BuildSettingsServiceAsync();
        Ioc.Default.ConfigureServices(
            new ServiceCollection()
                .AddSingleton<ISettingsService>(settingsService)
                .AddSingleton<IFileService, Win32FileService>()
                .AddSingleton<IWeakAurasService, WeakAurasLuaService>()
                .BuildServiceProvider());
    }

    protected async void Application_Startup(object sender, StartupEventArgs e)
    {
        ShutdownMode = ShutdownMode.OnLastWindowClose;
        await SetupDependencies();
        MdiParentWindow = new MainWindow();
        MainWindow = MdiParentWindow;
        var viewModel = new MainWindowViewModel("WeakAuras Interface");
        viewModel.RequestClose += MainWindowOnRequestClose;
        MdiParentWindow.DataContext = viewModel;
        MdiParentWindow.Show();
    }

    protected void Application_Exit(object sender, ExitEventArgs e)
    {
    }

    private void MainWindowOnRequestClose(object? sender, EventArgs e)
    {
        MdiParentWindow?.Close();
    }
}