namespace MyCloudSync.Core.Models;

public class Conflict
{
    public int Id { get; set; }
    public int FileStateId { get; set; }
    public string ConflictCopyName { get; set; } = "";
    public DateTime LocalModifiedAt { get; set; }
    public DateTime DriveModifiedAt { get; set; }
    public DateTime DetectedAt { get; set; }
    public bool IsResolved { get; set; }
}
