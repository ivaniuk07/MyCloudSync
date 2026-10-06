using System.Security.Cryptography;

namespace MyCloudSync.Infrastructure.FileSystem;

public class HashCalculator
{
    public string ComputeMd5(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        var hash = MD5.HashData(stream);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
