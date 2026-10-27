using MyCloudSync.Infrastructure.FileSystem;

namespace MyCloudSync.Tests;

public class TestFileSystem : LocalFileSystem
{
    public TestFileSystem() : base(new HashCalculator())
    {
    }

    public override void MoveToRecycleBin(string rootPath, string relativePath)
    {
        File.Delete(Path.Combine(rootPath, relativePath));
    }
}
