namespace MyCloudSync.Core.Models;

public enum SyncStatus
{
    NotConnected,
    Idle,
    Syncing,
    Paused,
    Offline,
    Error
}
