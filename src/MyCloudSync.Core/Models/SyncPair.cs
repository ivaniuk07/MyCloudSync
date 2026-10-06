namespace MyCloudSync.Core.Models;

public class SyncPair
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public string LocalPath { get; set; } = "";
    public string DriveFolderId { get; set; } = "";
    public string? DriveFolderName { get; set; }
    public SyncMode Mode { get; set; } = SyncMode.TwoWay;
    public bool IsActive { get; set; } = true;
    public string? PageToken { get; set; }
    public DateTime? LastSyncAt { get; set; }
}
