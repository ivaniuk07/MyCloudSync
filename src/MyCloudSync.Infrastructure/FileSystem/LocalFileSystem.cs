using MyCloudSync.Core.Abstractions;
using MyCloudSync.Core.Models;

namespace MyCloudSync.Infrastructure.FileSystem;

public class LocalFileSystem : ILocalFileSystem
{
    private readonly HashCalculator _hashCalculator;

    public LocalFileSystem(HashCalculator hashCalculator)
    {
        _hashCalculator = hashCalculator;
    }

    public IReadOnlyList<LocalFile> Scan(string rootPath)
    {
        throw new NotImplementedException();
    }

    public Stream OpenRead(string rootPath, string relativePath)
    {
        throw new NotImplementedException();
    }

    public Stream Create(string rootPath, string relativePath)
    {
        throw new NotImplementedException();
    }

    public void Rename(string rootPath, string relativePath, string newName)
    {
        throw new NotImplementedException();
    }

    public void MoveToRecycleBin(string rootPath, string relativePath)
    {
        throw new NotImplementedException();
    }
}
