using MyCloudSync.Infrastructure.FileSystem;
using Xunit;

namespace MyCloudSync.Tests;

public class FolderWatcherTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"mycloudsync-watch-{Guid.NewGuid()}");

    public FolderWatcherTests()
    {
        Directory.CreateDirectory(_root);
    }

    [Fact]
    public void Start_ReportsNewFile()
    {
        using var watcher = new FolderWatcher();
        using var changed = new ManualResetEventSlim();
        string? changedPath = null;
        watcher.FileChanged += path =>
        {
            changedPath = path;
            changed.Set();
        };

        watcher.Start(_root);
        File.WriteAllText(Path.Combine(_root, "new.txt"), "data");

        Assert.True(changed.Wait(TimeSpan.FromSeconds(5)));
        Assert.EndsWith("new.txt", changedPath);
    }

    [Fact]
    public void Stop_NoMoreEvents()
    {
        using var watcher = new FolderWatcher();
        var count = 0;
        watcher.FileChanged += path => count++;

        watcher.Start(_root);
        watcher.Stop();
        File.WriteAllText(Path.Combine(_root, "new.txt"), "data");
        Thread.Sleep(500);

        Assert.Equal(0, count);
    }

    public void Dispose()
    {
        Directory.Delete(_root, true);
    }
}
