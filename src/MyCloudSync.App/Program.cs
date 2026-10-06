using Microsoft.Extensions.DependencyInjection;
using MyCloudSync.App.Forms;
using MyCloudSync.Core.Abstractions;
using MyCloudSync.Core.Sync;
using MyCloudSync.Infrastructure.Data;
using MyCloudSync.Infrastructure.Data.Repositories;
using MyCloudSync.Infrastructure.FileSystem;
using MyCloudSync.Infrastructure.GoogleDrive;
using MyCloudSync.Infrastructure.Platform;

namespace MyCloudSync.App;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        Directory.CreateDirectory(AppPaths.DataFolder);

        var services = new ServiceCollection();
        ConfigureServices(services);
        using var provider = services.BuildServiceProvider();

        provider.GetRequiredService<DatabaseInitializer>().Initialize();

        Application.Run(provider.GetRequiredService<MainForm>());
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton(new SqliteConnectionFactory(AppPaths.DatabaseFile));
        services.AddSingleton<DatabaseInitializer>();

        services.AddSingleton<IAccountRepository, AccountRepository>();
        services.AddSingleton<ISyncPairRepository, SyncPairRepository>();
        services.AddSingleton<IFileStateRepository, FileStateRepository>();
        services.AddSingleton<IConflictRepository, ConflictRepository>();
        services.AddSingleton<ISyncLogRepository, SyncLogRepository>();
        services.AddSingleton<ISettingsRepository, SettingsRepository>();

        services.AddSingleton<ITokenProtector, DpapiTokenProtector>();
        services.AddSingleton<IAutostartService, AutostartService>();
        services.AddSingleton<IAuthService, GoogleAuthService>();
        services.AddSingleton<ICloudStorage, GoogleDriveStorage>();

        services.AddSingleton<HashCalculator>();
        services.AddSingleton<ILocalFileSystem, LocalFileSystem>();
        services.AddSingleton<IFolderWatcher, FolderWatcher>();

        services.AddSingleton<ChangeDetector>();
        services.AddSingleton<ExcludeMatcher>();
        services.AddSingleton<SyncPlanner>();
        services.AddSingleton<ConflictResolver>();
        services.AddSingleton<SyncScheduler>();
        services.AddSingleton<SyncCoordinator>();

        services.AddSingleton<MainForm>();
        services.AddTransient<SettingsForm>();
        services.AddTransient<LogForm>();
        services.AddTransient<DriveFolderPickerForm>();
    }
}
