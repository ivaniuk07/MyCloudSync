using MyCloudSync.Core.Abstractions;
using MyCloudSync.Core.Models;

namespace MyCloudSync.Core.Sync;

public class SyncCoordinator
{
    private readonly ICloudStorage _cloudStorage;
    private readonly ILocalFileSystem _localFileSystem;
    private readonly ISyncPairRepository _pairRepository;
    private readonly IFileStateRepository _fileStateRepository;
    private readonly ISyncLogRepository _logRepository;
    private readonly SyncPlanner _planner;

    public SyncCoordinator(
        ICloudStorage cloudStorage,
        ILocalFileSystem localFileSystem,
        ISyncPairRepository pairRepository,
        IFileStateRepository fileStateRepository,
        ISyncLogRepository logRepository,
        SyncPlanner planner)
    {
        _cloudStorage = cloudStorage;
        _localFileSystem = localFileSystem;
        _pairRepository = pairRepository;
        _fileStateRepository = fileStateRepository;
        _logRepository = logRepository;
        _planner = planner;
    }

    public SyncStatus Status { get; private set; } = SyncStatus.NotConnected;

    public event Action<SyncStatus>? StatusChanged;

    public Task RunSyncAsync()
    {
        throw new NotImplementedException();
    }

    public void Pause()
    {
        SetStatus(SyncStatus.Paused);
    }

    public void Resume()
    {
        SetStatus(SyncStatus.Idle);
    }

    private void SetStatus(SyncStatus status)
    {
        Status = status;
        StatusChanged?.Invoke(status);
    }
}
