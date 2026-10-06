using MyCloudSync.Core.Models;

namespace MyCloudSync.Core.Abstractions;

public interface ISyncPairRepository
{
    SyncPair? GetActive();
    void Save(SyncPair pair);
    IReadOnlyList<ExcludeRule> GetExcludeRules(int syncPairId);
    void SaveExcludeRules(int syncPairId, IEnumerable<string> patterns);
}
