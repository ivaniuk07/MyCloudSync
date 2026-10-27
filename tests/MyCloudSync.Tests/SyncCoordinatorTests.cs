using MyCloudSync.Core.Models;
using MyCloudSync.Core.Sync;
using MyCloudSync.Infrastructure.Data.Repositories;
using Xunit;

namespace MyCloudSync.Tests;

public class SyncCoordinatorTests : IDisposable
{
    private readonly TestDatabase _database = new();
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"mycloudsync-sync-{Guid.NewGuid()}");
    private readonly InMemoryCloudStorage _cloud = new();
    private readonly FileStateRepository _fileStates;
    private readonly ConflictRepository _conflicts;
    private readonly SyncLogRepository _log;
    private readonly SyncCoordinator _coordinator;
    private readonly SyncPair _pair;

    public SyncCoordinatorTests()
    {
        Directory.CreateDirectory(_root);
        var factory = _database.ConnectionFactory;
        var pairs = new SyncPairRepository(factory);
        _fileStates = new FileStateRepository(factory);
        _conflicts = new ConflictRepository(factory);
        _log = new SyncLogRepository(factory);

        _pair = _database.CreatePair();
        _pair.LocalPath = _root;
        pairs.Save(_pair);

        _coordinator = new SyncCoordinator(
            _cloud,
            new TestFileSystem(),
            pairs,
            _fileStates,
            _conflicts,
            _log,
            new SyncPlanner(new ChangeDetector(), new ExcludeMatcher()),
            new ConflictResolver());
    }

    private string LocalPath(string relativePath) => Path.Combine(_root, relativePath.Replace('/', Path.DirectorySeparatorChar));

    private void WriteLocal(string relativePath, string text)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(LocalPath(relativePath))!);
        File.WriteAllText(LocalPath(relativePath), text);
    }

    [Fact]
    public async Task FirstSync_UploadsLocalAndDownloadsCloudFiles()
    {
        WriteLocal("docs/local.txt", "from pc");
        _cloud.AddFile("photos/cloud.txt", "from drive");

        await _coordinator.RunSyncAsync();

        Assert.Equal(SyncStatus.Idle, _coordinator.Status);
        Assert.Equal("from pc", _cloud.ReadFile("docs/local.txt"));
        Assert.Equal("from drive", File.ReadAllText(LocalPath("photos/cloud.txt")));
        Assert.Equal(2, _fileStates.GetByPair(_pair.Id).Count);
        Assert.Equal(2, _log.GetRecent(10).Count);
    }

    [Fact]
    public async Task SecondSync_WithoutChanges_DoesNothing()
    {
        WriteLocal("a.txt", "text");
        await _coordinator.RunSyncAsync();

        await _coordinator.RunSyncAsync();

        Assert.Single(_log.GetRecent(10));
    }

    [Fact]
    public async Task ChangedLocalFile_IsUploadedAgain()
    {
        WriteLocal("a.txt", "v1");
        await _coordinator.RunSyncAsync();

        WriteLocal("a.txt", "v2");
        await _coordinator.RunSyncAsync();

        Assert.Equal("v2", _cloud.ReadFile("a.txt"));
    }

    [Fact]
    public async Task ChangedCloudFile_IsDownloaded()
    {
        _cloud.AddFile("a.txt", "v1");
        await _coordinator.RunSyncAsync();

        _cloud.ChangeFile("a.txt", "v2");
        await _coordinator.RunSyncAsync();

        Assert.Equal("v2", File.ReadAllText(LocalPath("a.txt")));
    }

    [Fact]
    public async Task DeletedLocalFile_IsTrashedInCloud()
    {
        WriteLocal("a.txt", "text");
        await _coordinator.RunSyncAsync();

        File.Delete(LocalPath("a.txt"));
        await _coordinator.RunSyncAsync();

        Assert.Equal("a.txt", Assert.Single(_cloud.TrashedPaths));
        Assert.Empty(_fileStates.GetByPair(_pair.Id));
    }

    [Fact]
    public async Task DeletedCloudFile_IsDeletedLocally()
    {
        WriteLocal("a.txt", "text");
        await _coordinator.RunSyncAsync();

        await _cloud.TrashAsync(_fileStates.GetByPair(_pair.Id)[0].DriveFileId!);
        await _coordinator.RunSyncAsync();

        Assert.False(File.Exists(LocalPath("a.txt")));
    }

    [Fact]
    public async Task Conflict_KeepsBothVersions()
    {
        WriteLocal("report.txt", "v1");
        await _coordinator.RunSyncAsync();
        string? conflictPath = null;
        _coordinator.ConflictDetected += path => conflictPath = path;

        WriteLocal("report.txt", "local edit");
        _cloud.ChangeFile("report.txt", "cloud edit");
        await _coordinator.RunSyncAsync();

        var copy = Directory.GetFiles(_root, "report (конфлікт*").Single();
        Assert.Equal("local edit", File.ReadAllText(copy));
        Assert.Equal("cloud edit", File.ReadAllText(LocalPath("report.txt")));
        Assert.Equal("local edit", _cloud.ReadFile(Path.GetFileName(copy)));
        Assert.Equal("report.txt", conflictPath);
        Assert.Single(_conflicts.GetOpen());
    }

    [Fact]
    public async Task ExcludedFile_IsNotUploaded()
    {
        new SyncPairRepository(_database.ConnectionFactory).SaveExcludeRules(_pair.Id, new[] { "*.tmp" });
        WriteLocal("temp.tmp", "x");

        await _coordinator.RunSyncAsync();

        Assert.Null(_cloud.ReadFile("temp.tmp"));
    }

    [Fact]
    public async Task Paused_DoesNotSync()
    {
        WriteLocal("a.txt", "text");
        _coordinator.Pause();

        await _coordinator.RunSyncAsync();

        Assert.Null(_cloud.ReadFile("a.txt"));
        Assert.Equal(SyncStatus.Paused, _coordinator.Status);
    }

    [Fact]
    public async Task NoInternet_SetsOfflineStatus()
    {
        _cloud.IsOffline = true;

        await _coordinator.RunSyncAsync();

        Assert.Equal(SyncStatus.Offline, _coordinator.Status);
    }

    [Fact]
    public async Task MissingLocalFolder_SetsErrorAndKeepsCloudFiles()
    {
        _cloud.AddFile("a.txt", "text");
        await _coordinator.RunSyncAsync();
        Directory.Delete(_root, true);

        await _coordinator.RunSyncAsync();

        Assert.Equal(SyncStatus.Error, _coordinator.Status);
        Assert.Empty(_cloud.TrashedPaths);
        Directory.CreateDirectory(_root);
    }

    [Fact]
    public async Task Progress_IsReported()
    {
        WriteLocal("a.txt", "1");
        WriteLocal("b.txt", "2");
        var reports = new List<(int Done, int Total)>();
        _coordinator.ProgressChanged += (done, total) => reports.Add((done, total));

        await _coordinator.RunSyncAsync();

        Assert.Equal((0, 2), reports[0]);
        Assert.Equal((2, 2), reports[^1]);
    }

    public void Dispose()
    {
        _database.Dispose();
        Directory.Delete(_root, true);
    }
}
