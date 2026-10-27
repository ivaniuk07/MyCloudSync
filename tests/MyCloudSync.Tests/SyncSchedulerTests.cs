using MyCloudSync.Core.Sync;
using Xunit;

namespace MyCloudSync.Tests;

public class SyncSchedulerTests
{
    [Fact]
    public void ManyLocalChanges_RequestOneSync()
    {
        using var scheduler = new SyncScheduler { DebounceDelay = TimeSpan.FromMilliseconds(300) };
        var requests = 0;
        scheduler.SyncRequested += () => Interlocked.Increment(ref requests);

        for (var i = 0; i < 5; i++)
        {
            scheduler.OnLocalChange("file.txt");
            Thread.Sleep(50);
        }

        Thread.Sleep(1000);

        Assert.Equal(1, requests);
    }

    [Fact]
    public void Start_RequestsSyncByInterval()
    {
        using var scheduler = new SyncScheduler();
        using var requested = new ManualResetEventSlim();
        scheduler.SyncRequested += () => requested.Set();

        scheduler.Start(TimeSpan.FromMilliseconds(200));

        Assert.True(requested.Wait(TimeSpan.FromSeconds(3)));
        scheduler.Stop();
    }

    [Fact]
    public void Stop_CancelsPendingSync()
    {
        using var scheduler = new SyncScheduler { DebounceDelay = TimeSpan.FromMilliseconds(300) };
        var requests = 0;
        scheduler.SyncRequested += () => Interlocked.Increment(ref requests);

        scheduler.OnLocalChange("file.txt");
        scheduler.Stop();
        Thread.Sleep(700);

        Assert.Equal(0, requests);
    }
}
