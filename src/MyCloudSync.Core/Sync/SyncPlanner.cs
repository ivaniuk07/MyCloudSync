using MyCloudSync.Core.Models;

namespace MyCloudSync.Core.Sync;

public class SyncPlanner
{
    private readonly ChangeDetector _changeDetector;
    private readonly ExcludeMatcher _excludeMatcher;

    public SyncPlanner(ChangeDetector changeDetector, ExcludeMatcher excludeMatcher)
    {
        _changeDetector = changeDetector;
        _excludeMatcher = excludeMatcher;
    }

    public IReadOnlyList<SyncItem> BuildPlan(
        SyncPair pair,
        IReadOnlyList<LocalFile> localFiles,
        IReadOnlyList<CloudFile> cloudFiles,
        IReadOnlyList<FileState> storedStates,
        IReadOnlyList<ExcludeRule> rules)
    {
        throw new NotImplementedException();
    }
}
