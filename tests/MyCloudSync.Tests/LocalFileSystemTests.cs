using MyCloudSync.Infrastructure.FileSystem;
using Xunit;

namespace MyCloudSync.Tests;

public class LocalFileSystemTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"mycloudsync-files-{Guid.NewGuid()}");
    private readonly LocalFileSystem _fileSystem = new(new HashCalculator());

    public LocalFileSystemTests()
    {
        Directory.CreateDirectory(Path.Combine(_root, "docs"));
        File.WriteAllText(Path.Combine(_root, "a.txt"), "hello");
        File.WriteAllText(Path.Combine(_root, "docs", "b.txt"), "world");
    }

    [Fact]
    public void Scan_ReturnsFilesFromSubfoldersWithSlashPaths()
    {
        var files = _fileSystem.Scan(_root).OrderBy(file => file.RelativePath).ToList();

        Assert.Equal(2, files.Count);
        Assert.Equal("a.txt", files[0].RelativePath);
        Assert.Equal("docs/b.txt", files[1].RelativePath);
        Assert.Equal("5d41402abc4b2a76b9719d911017c592", files[0].Hash);
        Assert.Equal(5, files[0].Size);
    }

    [Fact]
    public void GetFile_ReturnsNullForMissingFile()
    {
        Assert.Null(_fileSystem.GetFile(_root, "missing.txt"));
        Assert.NotNull(_fileSystem.GetFile(_root, "docs/b.txt"));
    }

    [Fact]
    public void SaveFile_CreatesFoldersAndOverwrites()
    {
        var source = Path.Combine(_root, "a.txt");

        _fileSystem.SaveFile(_root, "new/deep/c.txt", source);
        _fileSystem.SaveFile(_root, "docs/b.txt", source);

        Assert.Equal("hello", File.ReadAllText(Path.Combine(_root, "new", "deep", "c.txt")));
        Assert.Equal("hello", File.ReadAllText(Path.Combine(_root, "docs", "b.txt")));
    }

    [Fact]
    public void Rename_MovesFile()
    {
        _fileSystem.Rename(_root, "docs/b.txt", "docs/b (копія).txt");

        Assert.False(File.Exists(Path.Combine(_root, "docs", "b.txt")));
        Assert.True(File.Exists(Path.Combine(_root, "docs", "b (копія).txt")));
    }

    [Fact]
    public void OpenRead_ReturnsFileContent()
    {
        using var stream = _fileSystem.OpenRead(_root, "docs/b.txt");
        using var reader = new StreamReader(stream);

        Assert.Equal("world", reader.ReadToEnd());
    }

    public void Dispose()
    {
        Directory.Delete(_root, true);
    }
}
