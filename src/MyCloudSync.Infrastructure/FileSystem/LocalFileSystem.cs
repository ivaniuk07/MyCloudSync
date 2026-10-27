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
        var files = new List<LocalFile>();
        foreach (var fullPath in Directory.EnumerateFiles(rootPath, "*", SearchOption.AllDirectories))
        {
            files.Add(ReadFile(rootPath, fullPath));
        }

        return files;
    }

    public LocalFile? GetFile(string rootPath, string relativePath)
    {
        var fullPath = GetFullPath(rootPath, relativePath);
        return File.Exists(fullPath) ? ReadFile(rootPath, fullPath) : null;
    }

    public Stream OpenRead(string rootPath, string relativePath)
    {
        var fullPath = GetFullPath(rootPath, relativePath);
        return new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
    }

    public void SaveFile(string rootPath, string relativePath, string sourceFile)
    {
        var fullPath = GetFullPath(rootPath, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.Copy(sourceFile, fullPath, true);
    }

    public void Rename(string rootPath, string relativePath, string newRelativePath)
    {
        var oldPath = GetFullPath(rootPath, relativePath);
        var newPath = GetFullPath(rootPath, newRelativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(newPath)!);
        File.Move(oldPath, newPath);
    }

    public virtual void MoveToRecycleBin(string rootPath, string relativePath)
    {
        var fullPath = GetFullPath(rootPath, relativePath);
        if (File.Exists(fullPath))
        {
            Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(
                fullPath,
                Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs,
                Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
        }
    }

    private LocalFile ReadFile(string rootPath, string fullPath)
    {
        var info = new FileInfo(fullPath);
        var relativePath = Path.GetRelativePath(rootPath, fullPath).Replace('\\', '/');

        try
        {
            var hash = _hashCalculator.ComputeMd5(fullPath);
            return new LocalFile(relativePath, hash, info.Length, info.LastWriteTimeUtc);
        }
        catch (IOException)
        {
            return new LocalFile(relativePath, null, info.Length, info.LastWriteTimeUtc);
        }
        catch (UnauthorizedAccessException)
        {
            return new LocalFile(relativePath, null, info.Length, info.LastWriteTimeUtc);
        }
    }

    private static string GetFullPath(string rootPath, string relativePath)
    {
        return Path.Combine(rootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
    }
}
