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
        var localByPath = ToMap(localFiles, file => file.RelativePath);
        var cloudByPath = ToMap(cloudFiles, file => file.RelativePath);
        var storedByPath = ToMap(storedStates.Where(state => !state.IsFolder), state => state.RelativePath);

        var allPaths = localByPath.Keys
            .Concat(cloudByPath.Keys)
            .Concat(storedByPath.Keys)
            .Distinct(StringComparer.OrdinalIgnoreCase);

        var plan = new List<SyncItem>();
        foreach (var path in allPaths)
        {
            if (_excludeMatcher.IsExcluded(path, rules))
            {
                continue;
            }

            localByPath.TryGetValue(path, out var local);
            cloudByPath.TryGetValue(path, out var cloud);
            storedByPath.TryGetValue(path, out var stored);

            if (local != null && local.IsBusy)
            {
                continue;
            }

            var action = ApplyMode(_changeDetector.Decide(local, cloud, stored), pair.Mode);
            if (action != SyncActionType.None)
            {
                plan.Add(new SyncItem(path, action, local, cloud, stored));
            }
        }

        return plan
            .OrderBy(item => IsRemoval(item.Action) ? 0 : 1)
            .ThenBy(item => item.RelativePath, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static SyncActionType ApplyMode(SyncActionType action, SyncMode mode)
    {
        if (mode == SyncMode.UploadOnly)
        {
            return action switch
            {
                SyncActionType.Download => SyncActionType.None,
                SyncActionType.DeleteLocal => SyncActionType.None,
                SyncActionType.Conflict => SyncActionType.Upload,
                _ => action
            };
        }

        if (mode == SyncMode.DownloadOnly)
        {
            return action switch
            {
                SyncActionType.Upload => SyncActionType.None,
                SyncActionType.DeleteRemote => SyncActionType.None,
                SyncActionType.Conflict => SyncActionType.Download,
                _ => action
            };
        }

        return action;
    }

    private static bool IsRemoval(SyncActionType action)
    {
        return action is SyncActionType.DeleteLocal or SyncActionType.DeleteRemote or SyncActionType.RemoveState;
    }

    private static Dictionary<string, T> ToMap<T>(IEnumerable<T> items, Func<T, string> getPath)
    {
        return items
            .GroupBy(getPath, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
    }
}
