using MyCloudSync.Core.Abstractions;
using MyCloudSync.Core.Models;

namespace MyCloudSync.Infrastructure.Data.Repositories;

public class ConflictRepository : IConflictRepository
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public ConflictRepository(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public void Add(Conflict conflict)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyList<Conflict> GetOpen()
    {
        throw new NotImplementedException();
    }
}
