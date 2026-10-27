using System.Globalization;

namespace MyCloudSync.Core.Sync;

public class ConflictResolver
{
    public string CreateConflictName(string relativePath, string machineName, DateTime time)
    {
        var slashIndex = relativePath.LastIndexOf('/');
        var folder = relativePath[..(slashIndex + 1)];
        var fileName = relativePath[(slashIndex + 1)..];

        var name = Path.GetFileNameWithoutExtension(fileName);
        var extension = Path.GetExtension(fileName);
        var stamp = time.ToString("yyyy-MM-dd HH-mm", CultureInfo.InvariantCulture);

        return $"{folder}{name} (конфлікт {machineName} {stamp}){extension}";
    }
}
