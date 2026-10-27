using System.Security.Cryptography;

namespace MyCloudSync.Infrastructure.FileSystem;

public class HashCalculator
{
    public string ComputeMd5(string filePath)
    {
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        var hash = MD5.HashData(stream);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
