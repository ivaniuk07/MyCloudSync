namespace MyCloudSync.Core.Abstractions;

public interface IFolderWatcher
{
    event Action<string>? FileChanged;
    void Start(string folderPath);
    void Stop();
}
