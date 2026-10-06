namespace MyCloudSync.Core.Models;

public class AppSettings
{
    public int CheckIntervalSec { get; set; } = 300;
    public bool Autostart { get; set; }
    public bool StartMinimized { get; set; } = true;
    public bool ShowNotifications { get; set; } = true;
    public string Language { get; set; } = "uk";
}
