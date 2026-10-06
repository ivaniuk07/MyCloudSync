namespace MyCloudSync.Core.Sync;

public class SyncScheduler
{
    public event Action? SyncRequested;

    public void Start(TimeSpan interval)
    {
        throw new NotImplementedException();
    }

    public void Stop()
    {
        throw new NotImplementedException();
    }

    public void OnLocalChange(string path)
    {
        throw new NotImplementedException();
    }
}
