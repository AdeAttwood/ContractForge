namespace ContractForge.Cli;

public static class GeneratorMatcher
{
    public static string? FindClosest(string input, IReadOnlyCollection<string> candidates)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(candidates);

        var closestMatch = candidates
            .Select(candidate => new
            {
                Candidate = candidate,
                Distance = ComputeLevenshteinDistance(input, candidate),
            })
            .OrderBy(x => x.Distance)
            .ThenBy(x => x.Candidate, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault();

        if (closestMatch is null)
        {
            return null;
        }

        var maxDistance = Math.Max(2, closestMatch.Candidate.Length / 4);
        return closestMatch.Distance <= maxDistance ? closestMatch.Candidate : null;
    }

    public static int ComputeLevenshteinDistance(string source, string target)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);

        var distances = new int[source.Length + 1, target.Length + 1];

        for (var i = 0; i <= source.Length; i++)
        {
            distances[i, 0] = i;
        }

        for (var j = 0; j <= target.Length; j++)
        {
            distances[0, j] = j;
        }

        for (var i = 1; i <= source.Length; i++)
        {
            for (var j = 1; j <= target.Length; j++)
            {
                var cost = char.ToLowerInvariant(source[i - 1]) == char.ToLowerInvariant(target[j - 1]) ? 0 : 1;

                distances[i, j] = Math.Min(
                    Math.Min(
                        distances[i - 1, j] + 1,
                        distances[i, j - 1] + 1),
                    distances[i - 1, j - 1] + cost);
            }
        }

        return distances[source.Length, target.Length];
    }
}
