namespace MyCloudSync.Core.Models;

public class FileState
{
    public int Id { get; set; }
    public int SyncPairId { get; set; }
    public string RelativePath { get; set; } = "";
    public string? DriveFileId { get; set; }
    public bool IsFolder { get; set; }
    public long? SizeBytes { get; set; }
    public string? LocalHash { get; set; }
    public string? DriveMd5 { get; set; }
    public DateTime? LocalModifiedAt { get; set; }
    public DateTime? DriveModifiedAt { get; set; }
    public DateTime? LastSyncedAt { get; set; }
    public FileStatus Status { get; set; } = FileStatus.Pending;
}
