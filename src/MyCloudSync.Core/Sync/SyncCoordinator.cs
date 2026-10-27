using MyCloudSync.Core.Abstractions;
using MyCloudSync.Core.Models;

namespace MyCloudSync.Core.Sync;

public class SyncCoordinator
{
    private readonly ICloudStorage _cloudStorage;
    private readonly ILocalFileSystem _localFileSystem;
    private readonly ISyncPairRepository _pairRepository;
    private readonly IFileStateRepository _fileStateRepository;
    private readonly IConflictRepository _conflictRepository;
    private readonly ISyncLogRepository _logRepository;
    private readonly SyncPlanner _planner;
    private readonly ConflictResolver _conflictResolver;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private bool _paused;
    private bool _runAgain;

    public SyncCoordinator(
        ICloudStorage cloudStorage,
        ILocalFileSystem localFileSystem,
        ISyncPairRepository pairRepository,
        IFileStateRepository fileStateRepository,
        IConflictRepository conflictRepository,
        ISyncLogRepository logRepository,
        SyncPlanner planner,
        ConflictResolver conflictResolver)
    {
        _cloudStorage = cloudStorage;
        _localFileSystem = localFileSystem;
        _pairRepository = pairRepository;
        _fileStateRepository = fileStateRepository;
        _conflictRepository = conflictRepository;
        _logRepository = logRepository;
        _planner = planner;
        _conflictResolver = conflictResolver;
    }

    public SyncStatus Status { get; private set; } = SyncStatus.NotConnected;

    public event Action<SyncStatus>? StatusChanged;

    public event Action<int, int>? ProgressChanged;

    public event Action<string>? ConflictDetected;

    public async Task RunSyncAsync()
    {
        if (_paused)
        {
            return;
        }

        var pair = _pairRepository.GetActive();
        if (pair == null)
        {
            SetStatus(SyncStatus.NotConnected);
            return;
        }

        if (!await _lock.WaitAsync(0))
        {
            _runAgain = true;
            return;
        }

        try
        {
            SetStatus(SyncStatus.Syncing);
            var hasErrors = await RunPlanAsync(pair);
            SetStatus(_paused ? SyncStatus.Paused : hasErrors ? SyncStatus.Error : SyncStatus.Idle);
        }
        catch (HttpRequestException)
        {
            Log(pair, "Error", null, null, LogResult.Warning, "Немає з'єднання з інтернетом");
            SetStatus(SyncStatus.Offline);
        }
        catch (Exception ex)
        {
            Log(pair, "Error", null, null, LogResult.Error, ex.Message);
            SetStatus(SyncStatus.Error);
        }
        finally
        {
            _lock.Release();
        }

        if (_runAgain)
        {
            _runAgain = false;
            await RunSyncAsync();
        }
    }

    public void Pause()
    {
        _paused = true;
        SetStatus(SyncStatus.Paused);
    }

    public void Resume()
    {
        _paused = false;
        Refresh();
    }

    public void Refresh()
    {
        if (_paused)
        {
            SetStatus(SyncStatus.Paused);
        }
        else
        {
            SetStatus(_pairRepository.GetActive() == null ? SyncStatus.NotConnected : SyncStatus.Idle);
        }
    }

    private async Task<bool> RunPlanAsync(SyncPair pair)
    {
        var localFiles = await Task.Run(() => _localFileSystem.Scan(pair.LocalPath));
        var cloudFiles = await _cloudStorage.ListFilesAsync(pair.DriveFolderId);
        var storedStates = _fileStateRepository.GetByPair(pair.Id);
        var rules = _pairRepository.GetExcludeRules(pair.Id);

        var plan = _planner.BuildPlan(pair, localFiles, cloudFiles, storedStates, rules);
        var hasErrors = false;
        ProgressChanged?.Invoke(0, plan.Count);

        for (var i = 0; i < plan.Count && !_paused; i++)
        {
            try
            {
                await ExecuteAsync(pair, plan[i]);
            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                hasErrors = true;
                Log(pair, "Error", null, null, LogResult.Error, $"{plan[i].RelativePath}: {ex.Message}");
            }

            ProgressChanged?.Invoke(i + 1, plan.Count);
        }

        pair.LastSyncAt = DateTime.UtcNow;
        _pairRepository.Save(pair);
        return hasErrors;
    }

    private async Task ExecuteAsync(SyncPair pair, SyncItem item)
    {
        switch (item.Action)
        {
            case SyncActionType.Upload:
                await UploadAsync(pair, item);
                break;
            case SyncActionType.Download:
                await DownloadAsync(pair, item);
                break;
            case SyncActionType.DeleteLocal:
                _localFileSystem.MoveToRecycleBin(pair.LocalPath, item.RelativePath);
                _fileStateRepository.Delete(item.Stored!.Id);
                Log(pair, "Delete", "FromCloud", null, LogResult.Success, item.RelativePath);
                break;
            case SyncActionType.DeleteRemote:
                await _cloudStorage.TrashAsync(item.Cloud!.Id);
                _fileStateRepository.Delete(item.Stored!.Id);
                Log(pair, "Delete", "ToCloud", null, LogResult.Success, item.RelativePath);
                break;
            case SyncActionType.Conflict:
                await ResolveConflictAsync(pair, item);
                break;
            case SyncActionType.UpdateState:
                SaveState(pair, item.Stored, item.RelativePath, item.Local!, item.Cloud!, FileStatus.Synced);
                break;
            case SyncActionType.RemoveState:
                _fileStateRepository.Delete(item.Stored!.Id);
                break;
        }
    }

    private async Task UploadAsync(SyncPair pair, SyncItem item)
    {
        CloudFile uploaded;
        using (var stream = _localFileSystem.OpenRead(pair.LocalPath, item.RelativePath))
        {
            uploaded = item.Cloud == null
                ? await _cloudStorage.UploadAsync(pair.DriveFolderId, item.RelativePath, stream)
                : await _cloudStorage.UpdateAsync(item.Cloud.Id, item.RelativePath, stream);
        }

        var state = SaveState(pair, item.Stored, item.RelativePath, item.Local!, uploaded, FileStatus.Synced);
        Log(pair, "Upload", "ToCloud", state.Id, LogResult.Success, item.RelativePath);
    }

    private async Task DownloadAsync(SyncPair pair, SyncItem item)
    {
        await DownloadToLocalAsync(pair, item.Cloud!, item.RelativePath);

        var local = _localFileSystem.GetFile(pair.LocalPath, item.RelativePath)!;
        var state = SaveState(pair, item.Stored, item.RelativePath, local, item.Cloud!, FileStatus.Synced);
        Log(pair, "Download", "FromCloud", state.Id, LogResult.Success, item.RelativePath);
    }

    private async Task ResolveConflictAsync(SyncPair pair, SyncItem item)
    {
        var copyPath = _conflictResolver.CreateConflictName(item.RelativePath, Environment.MachineName, DateTime.Now);
        _localFileSystem.Rename(pair.LocalPath, item.RelativePath, copyPath);

        CloudFile uploadedCopy;
        using (var stream = _localFileSystem.OpenRead(pair.LocalPath, copyPath))
        {
            uploadedCopy = await _cloudStorage.UploadAsync(pair.DriveFolderId, copyPath, stream);
        }

        var copy = _localFileSystem.GetFile(pair.LocalPath, copyPath)!;
        SaveState(pair, null, copyPath, copy, uploadedCopy, FileStatus.Synced);

        await DownloadToLocalAsync(pair, item.Cloud!, item.RelativePath);
        var downloaded = _localFileSystem.GetFile(pair.LocalPath, item.RelativePath)!;
        var state = SaveState(pair, item.Stored, item.RelativePath, downloaded, item.Cloud!, FileStatus.Conflict);

        _conflictRepository.Add(new Conflict
        {
            FileStateId = state.Id,
            ConflictCopyName = copyPath,
            LocalModifiedAt = item.Local!.ModifiedAt,
            DriveModifiedAt = item.Cloud!.ModifiedAt
        });

        Log(pair, "Conflict", null, state.Id, LogResult.Warning, $"{item.RelativePath} → {copyPath}");
        ConflictDetected?.Invoke(item.RelativePath);
    }

    private async Task DownloadToLocalAsync(SyncPair pair, CloudFile cloud, string relativePath)
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            using (var stream = File.Create(tempFile))
            {
                await _cloudStorage.DownloadAsync(cloud.Id, stream);
            }

            _localFileSystem.SaveFile(pair.LocalPath, relativePath, tempFile);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    private FileState SaveState(SyncPair pair, FileState? stored, string relativePath, LocalFile local, CloudFile cloud, FileStatus status)
    {
        var state = stored ?? new FileState { SyncPairId = pair.Id };
        state.RelativePath = relativePath;
        state.DriveFileId = cloud.Id;
        state.SizeBytes = local.Size;
        state.LocalHash = local.Hash;
        state.DriveMd5 = cloud.Md5;
        state.LocalModifiedAt = local.ModifiedAt;
        state.DriveModifiedAt = cloud.ModifiedAt;
        state.LastSyncedAt = DateTime.UtcNow;
        state.Status = status;

        _fileStateRepository.Save(state);
        return state;
    }

    private void Log(SyncPair pair, string action, string? direction, int? fileStateId, LogResult result, string message)
    {
        _logRepository.Add(new SyncLogEntry
        {
            SyncPairId = pair.Id,
            FileStateId = fileStateId,
            Action = action,
            Direction = direction,
            Result = result,
            Message = message
        });
    }

    private void SetStatus(SyncStatus status)
    {
        Status = status;
        StatusChanged?.Invoke(status);
    }
}
