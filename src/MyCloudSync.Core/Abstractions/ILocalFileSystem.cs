using MyCloudSync.Core.Models;

namespace MyCloudSync.Core.Abstractions;

public interface ILocalFileSystem
{
    IReadOnlyList<LocalFile> Scan(string rootPath);
    Stream OpenRead(string rootPath, string relativePath);
    Stream Create(string rootPath, string relativePath);
    void Rename(string rootPath, string relativePath, string newName);
    void MoveToRecycleBin(string rootPath, string relativePath);
}
