using Microsoft.Data.Sqlite;
using MyCloudSync.Core.Models;
using MyCloudSync.Infrastructure.Data;
using MyCloudSync.Infrastructure.Data.Repositories;

namespace MyCloudSync.Tests;

public class TestDatabase : IDisposable
{
    private readonly string _databaseFile = Path.Combine(Path.GetTempPath(), $"mycloudsync-{Guid.NewGuid()}.db");

    public TestDatabase()
    {
        ConnectionFactory = new SqliteConnectionFactory(_databaseFile);
        new DatabaseInitializer(ConnectionFactory).Initialize();
    }

    public SqliteConnectionFactory ConnectionFactory { get; }

    public SyncPair CreatePair()
    {
        var account = new Account { Email = "student@gmail.com", AccessToken = "a", RefreshToken = "r" };
        new AccountRepository(ConnectionFactory).Save(account);

        var pair = new SyncPair { AccountId = account.Id, LocalPath = @"C:\Docs", DriveFolderId = "folder1", DriveFolderName = "Docs" };
        new SyncPairRepository(ConnectionFactory).Save(pair);
        return pair;
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
