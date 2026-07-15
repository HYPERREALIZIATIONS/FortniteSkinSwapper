using Microsoft.Extensions.DependencyInjection;
using FortniteSwapper.Services;
using FortniteSwapper.ViewModels;
using FortniteSwapper.Views;
using System;
using System.IO;
using System.Windows;

namespace FortniteSwapper;

public partial class App : Application
{
    public IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        Directory.CreateDirectory(Paths.AppDataDir);
        EnsureSampleMapping();

        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();

        // Kick off the cosmetic catalog load in the background.
        _ = Services.GetRequiredService<ICatalogService>().LoadAsync();

        var main = new MainWindow
        {
            DataContext = Services.GetRequiredService<MainViewModel>()
        };
        main.Show();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<LogService>();
        services.AddSingleton<ILogService>(sp => sp.GetRequiredService<LogService>());

        services.AddSingleton<SettingsService>();
        services.AddSingleton<ISettingsService>(sp => sp.GetRequiredService<SettingsService>());

        services.AddSingleton<CacheService>();
        services.AddSingleton<ICacheService>(sp => sp.GetRequiredService<CacheService>());

        services.AddSingleton<MappingService>();
        services.AddSingleton<IMappingService>(sp => sp.GetRequiredService<MappingService>());

        services.AddSingleton<FortniteLocator>();
        services.AddSingleton<IFortniteLocator>(sp => sp.GetRequiredService<FortniteLocator>());

        services.AddSingleton<BackupService>();
        services.AddSingleton<IBackupService>(sp => sp.GetRequiredService<BackupService>());

        services.AddSingleton<CatalogService>();
        services.AddSingleton<ICatalogService>(sp => sp.GetRequiredService<CatalogService>());

        services.AddSingleton<ApiClient>();
        services.AddSingleton<IApiClient>(sp => sp.GetRequiredService<ApiClient>());

        services.AddSingleton<SwapService>();
        services.AddSingleton<ISwapService>(sp => sp.GetRequiredService<SwapService>());

        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IFolderPicker, WindowsFolderPicker>();

        services.AddSingleton<MainViewModel>();
        services.AddSingleton<HomeViewModel>();
        services.AddSingleton<BrowseViewModel>();
        services.AddSingleton<SwapViewModel>();
        services.AddSingleton<SettingsViewModel>();
    }

    private static void EnsureSampleMapping()
    {
        try
        {
            var dir = Path.Combine(Paths.AppDataDir, "mappings");
            Directory.CreateDirectory(dir);
            var dest = Path.Combine(dir, "sample.mappings.json");
            if (!File.Exists(dest))
            {
                var src = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "mappings", "sample.mappings.json");
                if (File.Exists(src))
                {
                    File.Copy(src, dest);
                }
            }
        }
        catch
        {
            // Non-fatal: the user can import mappings from Settings.
        }
    }
}
