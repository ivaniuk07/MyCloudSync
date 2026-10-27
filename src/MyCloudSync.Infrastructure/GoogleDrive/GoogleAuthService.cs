using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using MyCloudSync.Core.Abstractions;
using MyCloudSync.Core.Models;
using MyCloudSync.Infrastructure.Data;

namespace MyCloudSync.Infrastructure.GoogleDrive;

public class GoogleAuthService : IAuthService
{
    private readonly IAccountRepository _accountRepository;
    private readonly AccountTokenStore _tokenStore;
    private UserCredential? _credential;
    private DriveService? _driveService;

    public GoogleAuthService(IAccountRepository accountRepository, ITokenProtector tokenProtector)
    {
        _accountRepository = accountRepository;
        _tokenStore = new AccountTokenStore(accountRepository, tokenProtector);
    }

    public async Task<Account> SignInAsync()
    {
        _credential = await AuthorizeAsync();
        _driveService = CreateDriveService(_credential);

        var request = _driveService.About.Get();
        request.Fields = "user(emailAddress, displayName)";
        var about = await request.ExecuteAsync();

        var account = _accountRepository.GetCurrent() ?? new Account();
        account.Email = about.User.EmailAddress;
        account.DisplayName = about.User.DisplayName;
        _tokenStore.SaveToAccount(account);

        return account;
    }

    public async Task SignOutAsync(Account account)
    {
        if (_credential != null)
        {
            try
            {
                await _credential.RevokeTokenAsync(CancellationToken.None);
            }
            catch (Exception)
            {
            }
        }

        _credential = null;
        _driveService?.Dispose();
        _driveService = null;
        _accountRepository.Delete(account.Id);
        await _tokenStore.ClearAsync();
    }

    public async Task<DriveService> GetDriveServiceAsync()
    {
        if (_driveService != null)
        {
            return _driveService;
        }

        if (_accountRepository.GetCurrent() == null)
        {
            throw new InvalidOperationException("Спочатку увійдіть у Google.");
        }

        _credential = await AuthorizeAsync();
        _driveService = CreateDriveService(_credential);
        return _driveService;
    }

    private async Task<UserCredential> AuthorizeAsync()
    {
        if (!File.Exists(AppPaths.ClientSecretFile))
        {
            throw new FileNotFoundException("Не знайдено файл client_secret.json поруч із програмою. Без нього вхід через Google неможливий.");
        }

        var secrets = GoogleClientSecrets.FromFile(AppPaths.ClientSecretFile).Secrets;
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(3));

        try
        {
            return await GoogleWebAuthorizationBroker.AuthorizeAsync(
                secrets,
                new[] { DriveService.Scope.Drive },
                "user",
                timeout.Token,
                _tokenStore);
        }
        catch (OperationCanceledException)
        {
            throw new TimeoutException("Час на вхід у Google вичерпано. Спробуйте ще раз.");
        }
    }

    private static DriveService CreateDriveService(UserCredential credential)
    {
        return new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "MyCloudSync"
        });
    }
}
