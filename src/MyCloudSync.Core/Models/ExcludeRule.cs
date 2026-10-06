namespace MyCloudSync.Core.Models;

public class ExcludeRule
{
    public int Id { get; set; }
    public int SyncPairId { get; set; }
    public string Pattern { get; set; } = "";
    public bool IsEnabled { get; set; } = true;
}
