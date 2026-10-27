using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Util.Store;
using MyCloudSync.Core.Abstractions;
using MyCloudSync.Core.Models;

namespace MyCloudSync.Infrastructure.GoogleDrive;

public class AccountTokenStore : IDataStore
{
    private readonly IAccountRepository _accountRepository;
    private readonly ITokenProtector _tokenProtector;
    private TokenResponse? _newToken;

    public AccountTokenStore(IAccountRepository accountRepository, ITokenProtector tokenProtector)
    {
        _accountRepository = accountRepository;
        _tokenProtector = tokenProtector;
    }

    public Task StoreAsync<T>(string key, T value)
    {
        if (value is TokenResponse token)
        {
            _newToken = token;

            var account = _accountRepository.GetCurrent();
            if (account != null)
            {
                SaveToAccount(account);
            }
        }

        return Task.CompletedTask;
    }

    public Task<T> GetAsync<T>(string key)
    {
        var account = _accountRepository.GetCurrent();
        if (account == null || account.RefreshToken == "")
        {
            return Task.FromResult(default(T)!);
        }

        var secondsLeft = account.TokenExpiresAt == null
            ? 0
            : Math.Max(0, (long)(account.TokenExpiresAt.Value - DateTime.UtcNow).TotalSeconds);

        var token = new TokenResponse
        {
            AccessToken = _tokenProtector.Unprotect(account.AccessToken),
            RefreshToken = _tokenProtector.Unprotect(account.RefreshToken),
            TokenType = "Bearer",
            IssuedUtc = DateTime.UtcNow,
            ExpiresInSeconds = secondsLeft
        };

        return Task.FromResult((T)(object)token);
    }

    public Task DeleteAsync<T>(string key)
    {
        _newToken = null;
        return Task.CompletedTask;
    }

    public Task ClearAsync()
    {
        _newToken = null;
        return Task.CompletedTask;
    }

    public void SaveToAccount(Account account)
    {
        if (_newToken != null)
        {
            account.AccessToken = _tokenProtector.Protect(_newToken.AccessToken ?? "");
            if (!string.IsNullOrEmpty(_newToken.RefreshToken))
            {
                account.RefreshToken = _tokenProtector.Protect(_newToken.RefreshToken);
            }

            account.TokenExpiresAt = _newToken.IssuedUtc.AddSeconds(_newToken.ExpiresInSeconds ?? 0);
        }

        _accountRepository.Save(account);
    }
}
