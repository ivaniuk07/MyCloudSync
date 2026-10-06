using MyCloudSync.Core.Models;

namespace MyCloudSync.Core.Abstractions;

public interface IConflictRepository
{
    void Add(Conflict conflict);
    IReadOnlyList<Conflict> GetOpen();
}
