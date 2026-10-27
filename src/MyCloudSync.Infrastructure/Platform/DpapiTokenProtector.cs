using System.Security.Cryptography;
using System.Text;
using MyCloudSync.Core.Abstractions;

namespace MyCloudSync.Infrastructure.Platform;

public class DpapiTokenProtector : ITokenProtector
{
    public string Protect(string token)
    {
        if (token == "")
        {
            return "";
        }

        var bytes = Encoding.UTF8.GetBytes(token);
        var encrypted = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
        return Convert.ToBase64String(encrypted);
    }

    public string Unprotect(string protectedToken)
    {
        if (protectedToken == "")
        {
            return "";
        }

        var encrypted = Convert.FromBase64String(protectedToken);
        var bytes = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
        return Encoding.UTF8.GetString(bytes);
    }
}
