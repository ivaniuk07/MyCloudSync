using MyCloudSync.Core.Abstractions;

namespace MyCloudSync.Infrastructure.FileSystem;

public class FolderWatcher : IFolderWatcher
{
    public event Action<string>? FileChanged;

    public void Start(string folderPath)
    {
        throw new NotImplementedException();
    }

    public void Stop()
    {
        throw new NotImplementedException();
    }
}
