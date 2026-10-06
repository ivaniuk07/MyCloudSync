namespace MyCloudSync.Core.Abstractions;

public interface IAutostartService
{
    bool IsEnabled();
    void SetEnabled(bool enabled);
}
