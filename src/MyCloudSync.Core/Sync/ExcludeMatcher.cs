using System.Text.RegularExpressions;
using MyCloudSync.Core.Models;

namespace MyCloudSync.Core.Sync;

public class ExcludeMatcher
{
    public bool IsExcluded(string relativePath, IReadOnlyList<ExcludeRule> rules)
    {
        var parts = relativePath.Split('/');
        var fileName = parts[^1];
        var folders = parts[..^1];

        foreach (var rule in rules)
        {
            var pattern = rule.Pattern.Trim();
            if (!rule.IsEnabled || pattern == "")
            {
                continue;
            }

            if (pattern.EndsWith('/'))
            {
                var folderPattern = pattern.TrimEnd('/');
                if (folders.Any(folder => Matches(folder, folderPattern)))
                {
                    return true;
                }
            }
            else if (Matches(fileName, pattern))
            {
                return true;
            }
        }

        return false;
    }

    private static bool Matches(string name, string pattern)
    {
        var regex = "^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
        return Regex.IsMatch(name, regex, RegexOptions.IgnoreCase);
    }
}
