using MyCloudSync.Core.Models;

namespace MyCloudSync.Core.Abstractions;

public interface IAccountRepository
{
    Account? GetCurrent();
    void Save(Account account);
    void Delete(int accountId);
}
