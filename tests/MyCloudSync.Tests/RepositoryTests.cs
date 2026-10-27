using MyCloudSync.Core.Models;
using MyCloudSync.Infrastructure.Data.Repositories;
using Xunit;

namespace MyCloudSync.Tests;

public class RepositoryTests : IDisposable
{
    private readonly TestDatabase _database = new();

    [Fact]
    public void Account_SaveThenGet_ReturnsSameAccount()
    {
        var repository = new AccountRepository(_database.ConnectionFactory);
        var account = new Account
        {
            Email = "student@gmail.com",
            DisplayName = "Student",
            AccessToken = "access",
            RefreshToken = "refresh",
            TokenExpiresAt = new DateTime(2026, 10, 21, 12, 0, 0, DateTimeKind.Utc)
        };

        repository.Save(account);
        var loaded = repository.GetCurrent();

        Assert.NotNull(loaded);
        Assert.True(account.Id > 0);
        Assert.Equal("student@gmail.com", loaded!.Email);
        Assert.Equal("refresh", loaded.RefreshToken);
        Assert.Equal(account.TokenExpiresAt, loaded.TokenExpiresAt);
    }

    [Fact]
    public void Account_Delete_RemovesPairAndFileStates()
    {
        var pair = _database.CreatePair();
        var fileStates = new FileStateRepository(_database.ConnectionFactory);
        fileStates.Save(new FileState { SyncPairId = pair.Id, RelativePath = "a.txt" });

        new AccountRepository(_database.ConnectionFactory).Delete(pair.AccountId);

        Assert.Null(new AccountRepository(_database.ConnectionFactory).GetCurrent());
        Assert.Null(new SyncPairRepository(_database.ConnectionFactory).GetActive());
        Assert.Empty(fileStates.GetByPair(pair.Id));
    }

    [Fact]
    public void SyncPair_SaveThenUpdate_KeepsOneRow()
    {
        var pair = _database.CreatePair();
        var repository = new SyncPairRepository(_database.ConnectionFactory);

        pair.Mode = SyncMode.UploadOnly;
        pair.LastSyncAt = new DateTime(2026, 10, 21, 10, 0, 0, DateTimeKind.Utc);
        repository.Save(pair);
        var loaded = repository.GetActive();

        Assert.NotNull(loaded);
        Assert.Equal(pair.Id, loaded!.Id);
        Assert.Equal(SyncMode.UploadOnly, loaded.Mode);
        Assert.Equal(@"C:\Docs", loaded.LocalPath);
        Assert.Equal(pair.LastSyncAt, loaded.LastSyncAt);
    }

    [Fact]
    public void ExcludeRules_SaveReplacesOldRules()
    {
        var pair = _database.CreatePair();
        var repository = new SyncPairRepository(_database.ConnectionFactory);

        repository.SaveExcludeRules(pair.Id, new[] { "*.tmp", "~$*" });
        repository.SaveExcludeRules(pair.Id, new[] { "*.log", "*.log", "node_modules/" });
        var rules = repository.GetExcludeRules(pair.Id);

        Assert.Equal(2, rules.Count);
        Assert.Equal("*.log", rules[0].Pattern);
        Assert.Equal("node_modules/", rules[1].Pattern);
    }

    [Fact]
    public void FileState_SaveUpdateAndDelete()
    {
        var pair = _database.CreatePair();
        var repository = new FileStateRepository(_database.ConnectionFactory);
        var state = new FileState
        {
            SyncPairId = pair.Id,
            RelativePath = "docs/a.txt",
            DriveFileId = "drive1",
            SizeBytes = 5_000_000_000,
            LocalHash = "h1",
            DriveMd5 = "h1",
            Status = FileStatus.Synced
        };

        repository.Save(state);
        state.LocalHash = "h2";
        state.Status = FileStatus.Conflict;
        repository.Save(state);
        var loaded = Assert.Single(repository.GetByPair(pair.Id));

        Assert.Equal("h2", loaded.LocalHash);
        Assert.Equal(FileStatus.Conflict, loaded.Status);
        Assert.Equal(5_000_000_000, loaded.SizeBytes);

        repository.Delete(state.Id);
        Assert.Empty(repository.GetByPair(pair.Id));
    }

    [Fact]
    public void FileState_DeleteByPair_RemovesAllStates()
    {
        var pair = _database.CreatePair();
        var repository = new FileStateRepository(_database.ConnectionFactory);
        repository.Save(new FileState { SyncPairId = pair.Id, RelativePath = "a.txt" });
        repository.Save(new FileState { SyncPairId = pair.Id, RelativePath = "b.txt" });

        repository.DeleteByPair(pair.Id);

        Assert.Empty(repository.GetByPair(pair.Id));
    }

    [Fact]
    public void Conflict_AddThenGetOpen()
    {
        var pair = _database.CreatePair();
        var state = new FileState { SyncPairId = pair.Id, RelativePath = "a.txt" };
        new FileStateRepository(_database.ConnectionFactory).Save(state);
        var repository = new ConflictRepository(_database.ConnectionFactory);

        repository.Add(new Conflict
        {
            FileStateId = state.Id,
            ConflictCopyName = "a (конфлікт PC 2026-10-21 10-00).txt",
            LocalModifiedAt = DateTime.UtcNow,
            DriveModifiedAt = DateTime.UtcNow
        });
        var open = Assert.Single(repository.GetOpen());

        Assert.Equal(state.Id, open.FileStateId);
        Assert.False(open.IsResolved);
    }

    public void Dispose()
    {
        _database.Dispose();
    }
}
