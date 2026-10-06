using MyCloudSync.Core.Models;

namespace MyCloudSync.Core.Sync;

public class ChangeDetector
{
    public SyncActionType Decide(LocalFile? local, CloudFile? remote, FileState? stored)
    {
        throw new NotImplementedException();
    }
}
