using MyCloudSync.Core.Models;

namespace MyCloudSync.Core.Sync;

public class ChangeDetector
{
    public SyncActionType Decide(LocalFile? local, CloudFile? remote, FileState? stored)
    {
        if (stored == null)
        {
            return DecideNew(local, remote);
        }

        if (local == null && remote == null)
        {
            return SyncActionType.RemoveState;
        }

        var localChanged = local != null && !SameHash(local.Hash, stored.LocalHash);
        var remoteChanged = remote != null && !SameHash(remote.Md5, stored.DriveMd5);

        if (local == null)
        {
            return remoteChanged ? SyncActionType.Download : SyncActionType.DeleteRemote;
        }

        if (remote == null)
        {
            return localChanged ? SyncActionType.Upload : SyncActionType.DeleteLocal;
        }

        if (localChanged && remoteChanged)
        {
            return SameHash(local.Hash, remote.Md5) ? SyncActionType.UpdateState : SyncActionType.Conflict;
        }

        if (localChanged)
        {
            return SyncActionType.Upload;
        }

        if (remoteChanged)
        {
            return SyncActionType.Download;
        }

        return SyncActionType.None;
    }

    private static SyncActionType DecideNew(LocalFile? local, CloudFile? remote)
    {
        if (local != null && remote != null)
        {
            return SameHash(local.Hash, remote.Md5) ? SyncActionType.UpdateState : SyncActionType.Conflict;
        }

        if (local != null)
        {
            return SyncActionType.Upload;
        }

        if (remote != null)
        {
            return SyncActionType.Download;
        }

        return SyncActionType.None;
    }

    private static bool SameHash(string? first, string? second)
    {
        return string.Equals(first, second, StringComparison.OrdinalIgnoreCase);
    }
}
