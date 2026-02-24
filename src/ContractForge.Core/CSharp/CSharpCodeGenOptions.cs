namespace ContractForge.Core.CSharp;

public class CSharpCodeGenOptions
{
    public CSharpCodeGenOptions()
    {
    }

    public CSharpCodeGenOptions(IReadOnlyDictionary<string, string> options)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var allowedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "partial"
        };

        foreach (var key in options.Keys)
        {
            if (!allowedKeys.Contains(key))
            {
                throw new ArgumentException($"Unknown C# generator option '{key}'.");
            }
        }

        if (options.TryGetValue("partial", out var value))
        {
            if (!bool.TryParse(value, out var parsed))
            {
                throw new ArgumentException($"Invalid value for 'partial': '{value}'. Expected true or false.");
            }

            UsePartialClasses = parsed;
        }
    }

    public bool UsePartialClasses { get; init; }
}