namespace MyCloudSync.Core.Models;

public enum FileStatus
{
    Pending,
    Synced,
    Error,
    Conflict,
    Skipped
}
