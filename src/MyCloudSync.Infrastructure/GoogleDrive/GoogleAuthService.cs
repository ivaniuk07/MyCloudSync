using MyCloudSync.Core.Abstractions;
using MyCloudSync.Core.Models;

namespace MyCloudSync.Infrastructure.GoogleDrive;

public class GoogleAuthService : IAuthService
{
    private readonly ITokenProtector _tokenProtector;

    public GoogleAuthService(ITokenProtector tokenProtector)
    {
        _tokenProtector = tokenProtector;
    }

    public Task<Account> SignInAsync()
    {
        throw new NotImplementedException();
    }

    public Task SignOutAsync(Account account)
    {
        throw new NotImplementedException();
    }
}
