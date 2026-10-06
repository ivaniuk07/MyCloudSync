namespace MyCloudSync.Core.Abstractions;

public interface ITokenProtector
{
    string Protect(string token);
    string Unprotect(string protectedToken);
}
