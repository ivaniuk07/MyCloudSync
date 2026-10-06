using MyCloudSync.Core.Abstractions;
using MyCloudSync.Core.Models;

namespace MyCloudSync.Infrastructure.Data.Repositories;

public class SyncPairRepository : ISyncPairRepository
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public SyncPairRepository(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public SyncPair? GetActive()
    {
        throw new NotImplementedException();
    }

    public void Save(SyncPair pair)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyList<ExcludeRule> GetExcludeRules(int syncPairId)
    {
        throw new NotImplementedException();
    }

    public void SaveExcludeRules(int syncPairId, IEnumerable<string> patterns)
    {
        throw new NotImplementedException();
    }
}
