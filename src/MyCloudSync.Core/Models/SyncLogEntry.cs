namespace MyCloudSync.Core.Models;

public class SyncLogEntry
{
    public int Id { get; set; }
    public int SyncPairId { get; set; }
    public int? FileStateId { get; set; }
    public string Action { get; set; } = "";
    public string? Direction { get; set; }
    public LogResult Result { get; set; }
    public string? Message { get; set; }
    public DateTime CreatedAt { get; set; }
}
