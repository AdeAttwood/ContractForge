using Inflector;

namespace ContractForge.Core.Types;

public class Service : BaseType
{
    public required string Identifier { get; set; }
    public string? Description { get; set; }
    public Dictionary<string, Function> Functions { get; set; } = new();
    public Dictionary<string, string> Attributes { get; set; } = new();

    public string Url()
    {
        var baseUrl = Attributes.GetValueOrDefault("http.baseUrl", "rpc").Trim().Trim('/');
        var servicePath = $"{this.Identifier.Underscore().Dasherize().ToLower()}-service";

        return string.IsNullOrWhiteSpace(baseUrl)
            ? $"/{servicePath}"
            : $"/{baseUrl}/{servicePath}";
    }
}