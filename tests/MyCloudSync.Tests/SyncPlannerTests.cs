using MyCloudSync.Core.Models;
using MyCloudSync.Core.Sync;
using Xunit;

namespace MyCloudSync.Tests;

public class SyncPlannerTests
{
    private readonly SyncPlanner _planner = new(new ChangeDetector(), new ExcludeMatcher());
    private readonly List<ExcludeRule> _noRules = new();

    private static LocalFile Local(string path, string? hash = "h1") => new(path, hash, 10, DateTime.UtcNow);

    private static CloudFile Cloud(string path, string md5 = "h1") => new("id-" + path, path, md5, 10, DateTime.UtcNow);

    private static FileState Stored(string path) => new() { RelativePath = path, LocalHash = "h1", DriveMd5 = "h1" };

    [Fact]
    public void BuildPlan_CreatesItemForEveryChange()
    {
        var pair = new SyncPair();
        var local = new List<LocalFile> { Local("new.txt"), Local("same.txt") };
        var cloud = new List<CloudFile> { Cloud("same.txt"), Cloud("fromcloud.txt") };
        var stored = new List<FileState> { Stored("same.txt") };

        var plan = _planner.BuildPlan(pair, local, cloud, stored, _noRules);

        Assert.Equal(2, plan.Count);
        Assert.Contains(plan, item => item.RelativePath == "new.txt" && item.Action == SyncActionType.Upload);
        Assert.Contains(plan, item => item.RelativePath == "fromcloud.txt" && item.Action == SyncActionType.Download);
    }

    [Fact]
    public void BuildPlan_SkipsExcludedFiles()
    {
        var local = new List<LocalFile> { Local("notes.tmp"), Local("notes.txt") };
        var rules = new List<ExcludeRule> { new() { Pattern = "*.tmp" } };

        var plan = _planner.BuildPlan(new SyncPair(), local, new List<CloudFile>(), new List<FileState>(), rules);

        Assert.Single(plan);
        Assert.Equal("notes.txt", plan[0].RelativePath);
    }

    [Fact]
    public void BuildPlan_SkipsBusyFiles()
    {
        var local = new List<LocalFile> { Local("open.docx", null) };
        var cloud = new List<CloudFile> { Cloud("open.docx") };
        var stored = new List<FileState> { Stored("open.docx") };

        var plan = _planner.BuildPlan(new SyncPair(), local, cloud, stored, _noRules);

        Assert.Empty(plan);
    }

    [Fact]
    public void BuildPlan_UploadOnly_NeverChangesLocalFolder()
    {
        var pair = new SyncPair { Mode = SyncMode.UploadOnly };
        var local = new List<LocalFile> { Local("conflict.txt", "h2"), Local("deleted-in-cloud.txt") };
        var cloud = new List<CloudFile> { Cloud("conflict.txt", "h3"), Cloud("fromcloud.txt") };
        var stored = new List<FileState> { Stored("conflict.txt"), Stored("deleted-in-cloud.txt") };

        var plan = _planner.BuildPlan(pair, local, cloud, stored, _noRules);

        Assert.Single(plan);
        Assert.Equal("conflict.txt", plan[0].RelativePath);
        Assert.Equal(SyncActionType.Upload, plan[0].Action);
    }

    [Fact]
    public void BuildPlan_DownloadOnly_NeverChangesCloud()
    {
        var pair = new SyncPair { Mode = SyncMode.DownloadOnly };
        var local = new List<LocalFile> { Local("new.txt") };
        var cloud = new List<CloudFile> { Cloud("deleted-locally.txt"), Cloud("fromcloud.txt") };
        var stored = new List<FileState> { Stored("deleted-locally.txt") };

        var plan = _planner.BuildPlan(pair, local, cloud, stored, _noRules);

        Assert.Single(plan);
        Assert.Equal("fromcloud.txt", plan[0].RelativePath);
        Assert.Equal(SyncActionType.Download, plan[0].Action);
    }

    [Fact]
    public void BuildPlan_PutsDeletesFirst()
    {
        var local = new List<LocalFile> { Local("a-new.txt") };
        var cloud = new List<CloudFile> { Cloud("z-old.txt") };
        var stored = new List<FileState> { Stored("z-old.txt") };

        var plan = _planner.BuildPlan(new SyncPair(), local, cloud, stored, _noRules);

        Assert.Equal(SyncActionType.DeleteRemote, plan[0].Action);
        Assert.Equal(SyncActionType.Upload, plan[1].Action);
    }

    [Fact]
    public void BuildPlan_ComparesPathsIgnoringCase()
    {
        var local = new List<LocalFile> { Local("Docs/Report.txt") };
        var cloud = new List<CloudFile> { Cloud("docs/report.txt") };
        var stored = new List<FileState> { Stored("docs/report.txt") };

        var plan = _planner.BuildPlan(new SyncPair(), local, cloud, stored, _noRules);

        Assert.Empty(plan);
    }
}
