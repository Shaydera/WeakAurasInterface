using System;
using System.IO;
using System.Linq;
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
        await SetupDependencies();
        (bool isSuccess, bool shouldShutdown, string message) argsResult = await HandleStartupArgsAsync(e.Args);
        if (!argsResult.isSuccess || argsResult.shouldShutdown)
        {
            if (!string.IsNullOrWhiteSpace(argsResult.message))
                MessageBox.Show(argsResult.message, "WeakAuras Interface Error", MessageBoxButton.OK, MessageBoxImage.Error);
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            Shutdown(argsResult.isSuccess ? 0 : 1);
            return;
        }
        
        MdiParentWindow = new MainWindow();
        MainWindow = MdiParentWindow;
        var viewModel = new MainWindowViewModel("WeakAuras Interface");
        viewModel.RequestClose += MainWindowOnRequestClose;
        MdiParentWindow.DataContext = viewModel;
        MdiParentWindow.Show();
    }

    private static async Task<(bool isSuccess, bool shouldShutdown, string message)> HandleStartupArgsAsync(string[] args)
    {
        if (args.Length == 0)
            return (true, false, string.Empty);

        var enforceShutdown = false;

        string? batchArg = args.FirstOrDefault(arg => arg.Equals("-batch"));
        if (!string.IsNullOrEmpty(batchArg))
        {
            if(!Ioc.Default.GetRequiredService<ISettingsService>().IsValid())
                return (false, true, "Invalid settings file. Please check your settings file and try again.");
            string? batchDir = args.ElementAtOrDefault(Array.IndexOf(args, batchArg) + 1);
            if (string.IsNullOrEmpty(batchDir) || !Directory.Exists(batchDir))
                return (false, true, "Invalid batch directory specified.");

            IBatchService batchService = new BatchService();
            (bool isSuccess, int exportCount) batchResult = await batchService.StartBatchExportAsync(batchDir);
            if (!batchResult.isSuccess)
                return (false, true, "Batch export failed.");
            enforceShutdown = true;
        }

        return (true, enforceShutdown, string.Empty);
    }

    protected void Application_Exit(object sender, ExitEventArgs e)
    {
    }

    private void MainWindowOnRequestClose(object? sender, EventArgs e)
    {
        MdiParentWindow?.Close();
    }
}