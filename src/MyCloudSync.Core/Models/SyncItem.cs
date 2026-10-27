namespace MyCloudSync.Core.Models;

public record SyncItem(string RelativePath, SyncActionType Action, LocalFile? Local, CloudFile? Cloud, FileState? Stored);
