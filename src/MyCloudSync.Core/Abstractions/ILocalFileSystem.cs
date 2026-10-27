using MyCloudSync.Core.Models;

namespace MyCloudSync.Core.Abstractions;

public interface ILocalFileSystem
{
    IReadOnlyList<LocalFile> Scan(string rootPath);
    LocalFile? GetFile(string rootPath, string relativePath);
    Stream OpenRead(string rootPath, string relativePath);
    void SaveFile(string rootPath, string relativePath, string sourceFile);
    void Rename(string rootPath, string relativePath, string newRelativePath);
    void MoveToRecycleBin(string rootPath, string relativePath);
}
