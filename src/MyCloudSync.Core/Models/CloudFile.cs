namespace MyCloudSync.Core.Models;

public record CloudFile(string Id, string RelativePath, string? Md5, long Size, DateTime ModifiedAt);
