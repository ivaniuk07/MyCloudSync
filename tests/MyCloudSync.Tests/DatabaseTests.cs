using Microsoft.Data.Sqlite;
using MyCloudSync.Infrastructure.Data;
using MyCloudSync.Infrastructure.Data.Repositories;
using Xunit;

namespace MyCloudSync.Tests;

public class DatabaseTests : IDisposable
{
    private readonly string _databaseFile = Path.Combine(Path.GetTempPath(), $"mycloudsync-{Guid.NewGuid()}.db");
    private readonly SqliteConnectionFactory _connectionFactory;

    public DatabaseTests()
    {
        _connectionFactory = new SqliteConnectionFactory(_databaseFile);
        new DatabaseInitializer(_connectionFactory).Initialize();
    }

    [Fact]
    public void Initialize_CreatesDefaultSettings()
    {
        var settings = new SettingsRepository(_connectionFactory).Load();

        Assert.Equal(300, settings.CheckIntervalSec);
        Assert.False(settings.Autostart);
    }

    [Fact]
    public void Initialize_CanRunTwice()
    {
        new DatabaseInitializer(_connectionFactory).Initialize();

        var settings = new SettingsRepository(_connectionFactory).Load();

        Assert.Equal(300, settings.CheckIntervalSec);
    }

    [Fact]
    public void SaveSettings_ThenLoad_ReturnsSavedValues()
    {
        var repository = new SettingsRepository(_connectionFactory);
        var settings = repository.Load();
        settings.CheckIntervalSec = 600;
        settings.StartMinimized = false;

        repository.Save(settings);
        var loaded = repository.Load();

        Assert.Equal(600, loaded.CheckIntervalSec);
        Assert.False(loaded.StartMinimized);
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        foreach (var file in Directory.GetFiles(Path.GetTempPath(), Path.GetFileName(_databaseFile) + "*"))
        {
            File.Delete(file);
        }
    }
}
