using MyCloudSync.Core.Abstractions;
using MyCloudSync.Core.Models;

namespace MyCloudSync.Infrastructure.Data.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public AccountRepository(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public Account? GetCurrent()
    {
        throw new NotImplementedException();
    }

    public void Save(Account account)
    {
        throw new NotImplementedException();
    }

    public void Delete(int accountId)
    {
        throw new NotImplementedException();
    }
}
