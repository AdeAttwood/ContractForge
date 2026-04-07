using ContractForge.Cli;

namespace ContractForge.Cli.Test;

public class GeneratorMatcherTest
{
    [Theory]
    [InlineData("typescript-client", "typescript-client", 0)]
    [InlineData("typescript-clien", "typescript-client", 1)]
    [InlineData("kitten", "sitting", 3)]
    [InlineData("gumbo", "gambol", 2)]
    public void ComputeLevenshteinDistance_ReturnsExpectedDistance(string source, string target, int expected)
    {
        var distance = GeneratorMatcher.ComputeLevenshteinDistance(source, target);

        Assert.Equal(expected, distance);
    }

    [Fact]
    public void FindClosest_WithCloseCandidate_ReturnsClosestMatch()
    {
        var candidates = new[] { "csharp-jsonapi", "typescript-client" };

        var closest = GeneratorMatcher.FindClosest("typescript-clien", candidates);

        Assert.Equal("typescript-client", closest);
    }

    [Fact]
    public void FindClosest_WithCaseDifference_ReturnsExactCandidate()
    {
        var candidates = new[] { "csharp-jsonapi", "typescript-client" };

        var closest = GeneratorMatcher.FindClosest("TYPESCRIPT-CLIENT", candidates);

        Assert.Equal("typescript-client", closest);
    }

    [Fact]
    public void FindClosest_WithDistantCandidate_ReturnsNull()
    {
        var candidates = new[] { "csharp-jsonapi", "typescript-client" };

        var closest = GeneratorMatcher.FindClosest("python-server", candidates);

        Assert.Null(closest);
    }
}