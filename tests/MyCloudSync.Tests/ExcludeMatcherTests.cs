using MyCloudSync.Core.Models;
using MyCloudSync.Core.Sync;
using Xunit;

namespace MyCloudSync.Tests;

public class ExcludeMatcherTests
{
    private readonly ExcludeMatcher _matcher = new();

    private static List<ExcludeRule> Rules(params string[] patterns) =>
        patterns.Select(pattern => new ExcludeRule { Pattern = pattern }).ToList();

    [Theory]
    [InlineData("notes.tmp", true)]
    [InlineData("docs/old/NOTES.TMP", true)]
    [InlineData("notes.txt", false)]
    [InlineData("~$report.docx", true)]
    [InlineData("node_modules/lib/index.js", true)]
    [InlineData("src/node_modules.txt", false)]
    public void IsExcluded_UsesMasks(string path, bool expected)
    {
        var rules = Rules("*.tmp", "~$*", "node_modules/");

        Assert.Equal(expected, _matcher.IsExcluded(path, rules));
    }

    [Fact]
    public void QuestionMark_MatchesOneCharacter()
    {
        var rules = Rules("file?.log");

        Assert.True(_matcher.IsExcluded("file1.log", rules));
        Assert.False(_matcher.IsExcluded("file12.log", rules));
    }

    [Fact]
    public void DisabledRule_IsIgnored()
    {
        var rules = new List<ExcludeRule> { new() { Pattern = "*.tmp", IsEnabled = false } };

        Assert.False(_matcher.IsExcluded("notes.tmp", rules));
    }

    [Fact]
    public void NoRules_NothingExcluded()
    {
        Assert.False(_matcher.IsExcluded("notes.tmp", new List<ExcludeRule>()));
    }
}
