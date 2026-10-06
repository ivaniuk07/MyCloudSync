using MyCloudSync.Core.Models;

namespace MyCloudSync.Core.Abstractions;

public interface IAuthService
{
    Task<Account> SignInAsync();
    Task SignOutAsync(Account account);
}
