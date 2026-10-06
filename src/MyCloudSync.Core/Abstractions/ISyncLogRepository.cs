using MyCloudSync.Core.Models;

namespace MyCloudSync.Core.Abstractions;

public interface ISyncLogRepository
{
    void Add(SyncLogEntry entry);
    IReadOnlyList<SyncLogEntry> GetRecent(int count);
    void Clear();
}
