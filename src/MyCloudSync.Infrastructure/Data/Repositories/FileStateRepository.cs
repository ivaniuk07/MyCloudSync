using MyCloudSync.Core.Abstractions;
using MyCloudSync.Core.Models;

namespace MyCloudSync.Infrastructure.Data.Repositories;

public class FileStateRepository : IFileStateRepository
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public FileStateRepository(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public IReadOnlyList<FileState> GetByPair(int syncPairId)
    {
        throw new NotImplementedException();
    }

    public void Save(FileState state)
    {
        throw new NotImplementedException();
    }

    public void Delete(int fileStateId)
    {
        throw new NotImplementedException();
    }
}
