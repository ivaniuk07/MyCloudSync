using MyCloudSync.Core.Models;
using MyCloudSync.Core.Sync;
using Xunit;

namespace MyCloudSync.Tests;

public class ChangeDetectorTests
{
    private readonly ChangeDetector _detector = new();

    private static LocalFile Local(string hash) => new("a.txt", hash, 10, DateTime.UtcNow);

    private static CloudFile Cloud(string md5) => new("id1", "a.txt", md5, 10, DateTime.UtcNow);

    private static FileState Stored(string hash) => new() { RelativePath = "a.txt", LocalHash = hash, DriveMd5 = hash };

    [Fact]
    public void NothingChanged_ReturnsNone()
    {
        Assert.Equal(SyncActionType.None, _detector.Decide(Local("h1"), Cloud("h1"), Stored("h1")));
    }

    [Fact]
    public void LocalChanged_ReturnsUpload()
    {
        Assert.Equal(SyncActionType.Upload, _detector.Decide(Local("h2"), Cloud("h1"), Stored("h1")));
    }

    [Fact]
    public void RemoteChanged_ReturnsDownload()
    {
        Assert.Equal(SyncActionType.Download, _detector.Decide(Local("h1"), Cloud("h2"), Stored("h1")));
    }

    [Fact]
    public void BothChangedToSameContent_ReturnsUpdateState()
    {
        Assert.Equal(SyncActionType.UpdateState, _detector.Decide(Local("h2"), Cloud("h2"), Stored("h1")));
    }

    [Fact]
    public void BothChangedDifferently_ReturnsConflict()
    {
        Assert.Equal(SyncActionType.Conflict, _detector.Decide(Local("h2"), Cloud("h3"), Stored("h1")));
    }

    [Fact]
    public void NewLocalFile_ReturnsUpload()
    {
        Assert.Equal(SyncActionType.Upload, _detector.Decide(Local("h1"), null, null));
    }

    [Fact]
    public void NewRemoteFile_ReturnsDownload()
    {
        Assert.Equal(SyncActionType.Download, _detector.Decide(null, Cloud("h1"), null));
    }

    [Fact]
    public void NewOnBothSidesWithSameContent_ReturnsUpdateState()
    {
        Assert.Equal(SyncActionType.UpdateState, _detector.Decide(Local("h1"), Cloud("h1"), null));
    }

    [Fact]
    public void NewOnBothSidesWithDifferentContent_ReturnsConflict()
    {
        Assert.Equal(SyncActionType.Conflict, _detector.Decide(Local("h1"), Cloud("h2"), null));
    }

    [Fact]
    public void DeletedLocally_ReturnsDeleteRemote()
    {
        Assert.Equal(SyncActionType.DeleteRemote, _detector.Decide(null, Cloud("h1"), Stored("h1")));
    }

    [Fact]
    public void DeletedInCloud_ReturnsDeleteLocal()
    {
        Assert.Equal(SyncActionType.DeleteLocal, _detector.Decide(Local("h1"), null, Stored("h1")));
    }

    [Fact]
    public void DeletedLocallyButChangedInCloud_ReturnsDownload()
    {
        Assert.Equal(SyncActionType.Download, _detector.Decide(null, Cloud("h2"), Stored("h1")));
    }

    [Fact]
    public void ChangedLocallyButDeletedInCloud_ReturnsUpload()
    {
        Assert.Equal(SyncActionType.Upload, _detector.Decide(Local("h2"), null, Stored("h1")));
    }

    [Fact]
    public void DeletedOnBothSides_ReturnsRemoveState()
    {
        Assert.Equal(SyncActionType.RemoveState, _detector.Decide(null, null, Stored("h1")));
    }
}
