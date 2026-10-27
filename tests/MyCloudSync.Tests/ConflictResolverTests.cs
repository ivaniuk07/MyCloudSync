using MyCloudSync.Core.Sync;
using Xunit;

namespace MyCloudSync.Tests;

public class ConflictResolverTests
{
    private readonly ConflictResolver _resolver = new();

    [Fact]
    public void CreateConflictName_AddsMachineAndTimeBeforeExtension()
    {
        var name = _resolver.CreateConflictName("docs/report.docx", "HOME-PC", new DateTime(2026, 10, 21, 14, 5, 0));

        Assert.Equal("docs/report (конфлікт HOME-PC 2026-10-21 14-05).docx", name);
    }

    [Fact]
    public void CreateConflictName_WorksWithoutFolderAndExtension()
    {
        var name = _resolver.CreateConflictName("README", "LAPTOP", new DateTime(2026, 1, 2, 3, 4, 0));

        Assert.Equal("README (конфлікт LAPTOP 2026-01-02 03-04)", name);
    }
}
