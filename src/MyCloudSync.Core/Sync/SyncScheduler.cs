namespace MyCloudSync.Core.Sync;

public class SyncScheduler : IDisposable
{
    private readonly Timer _intervalTimer;
    private readonly Timer _debounceTimer;

    public SyncScheduler()
    {
        _intervalTimer = new Timer(_ => SyncRequested?.Invoke());
        _debounceTimer = new Timer(_ => SyncRequested?.Invoke());
    }

    public event Action? SyncRequested;

    public TimeSpan DebounceDelay { get; set; } = TimeSpan.FromSeconds(2);

    public void Start(TimeSpan interval)
    {
        _intervalTimer.Change(interval, interval);
    }

    public void Stop()
    {
        _intervalTimer.Change(Timeout.Infinite, Timeout.Infinite);
        _debounceTimer.Change(Timeout.Infinite, Timeout.Infinite);
    }

    public void OnLocalChange(string path)
    {
        _debounceTimer.Change(DebounceDelay, Timeout.InfiniteTimeSpan);
    }

    public void Dispose()
    {
        _intervalTimer.Dispose();
        _debounceTimer.Dispose();
    }
}
