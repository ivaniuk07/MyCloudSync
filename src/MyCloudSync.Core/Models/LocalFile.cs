namespace MyCloudSync.Core.Models;

public record LocalFile(string RelativePath, string Hash, long Size, DateTime ModifiedAt);
