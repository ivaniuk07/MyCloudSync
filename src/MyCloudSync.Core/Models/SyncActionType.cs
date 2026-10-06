namespace MyCloudSync.Core.Models;

public enum SyncActionType
{
    None,
    Upload,
    Download,
    DeleteLocal,
    DeleteRemote,
    Conflict,
    UpdateState
}
