using MyCloudSync.Core.Abstractions;

namespace MyCloudSync.Infrastructure.FileSystem;

public class FolderWatcher : IFolderWatcher, IDisposable
{
    private FileSystemWatcher? _watcher;

    public event Action<string>? FileChanged;

    public void Start(string folderPath)
    {
        Stop();

        _watcher = new FileSystemWatcher(folderPath)
        {
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite | NotifyFilters.Size
        };

        _watcher.Created += OnChanged;
        _watcher.Changed += OnChanged;
        _watcher.Deleted += OnChanged;
        _watcher.Renamed += OnChanged;
        _watcher.Error += (sender, e) => FileChanged?.Invoke(folderPath);
        _watcher.EnableRaisingEvents = true;
    }

    public void Stop()
    {
        _watcher?.Dispose();
        _watcher = null;
    }

    public void Dispose()
    {
        Stop();
    }

    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        FileChanged?.Invoke(e.FullPath);
    }
}
