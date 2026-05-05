namespace ContractForge.Core.OpenApi;

public class OpenApiCodeGenOptions
{
    public OpenApiCodeGenOptions()
    {
    }

    public OpenApiCodeGenOptions(IReadOnlyDictionary<string, string> options)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var allowedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "openapi",
            "info.title",
            "info.version",
            "servers.0.url"
        };

        foreach (var key in options.Keys)
        {
            if (!allowedKeys.Contains(key))
            {
                throw new ArgumentException($"Unknown OpenAPI generator option '{key}'.");
            }
        }

        if (options.TryGetValue("openapi", out var openApiVersion))
        {
            OpenApiVersion = openApiVersion switch
            {
                "3.0" => "3.0.3",
                "3.0.3" => "3.0.3",
                "3.1" => "3.1.0",
                "3.1.0" => "3.1.0",
                _ => throw new ArgumentException($"Invalid value for 'openapi': '{openApiVersion}'. Expected 3.0.3 or 3.1.0.")
            };
        }

        if (options.TryGetValue("info.title", out var title))
        {
            Title = title;
        }

        if (options.TryGetValue("info.version", out var version))
        {
            ApiVersion = version;
        }

        if (options.TryGetValue("servers.0.url", out var serverUrl))
        {
            ServerUrl = serverUrl;
        }
    }

    public string OpenApiVersion { get; init; } = "3.0.3";
    public string Title { get; init; } = "ContractForge API";
    public string ApiVersion { get; init; } = "1.0.0";
    public string? ServerUrl { get; init; }
    public bool IsOpenApi31 => OpenApiVersion == "3.1.0";
}