using Inflector;

namespace NetRpc.Core.Types;

public class Function : BaseType
{
    public required string Identifier { get; set; }
    public string? Description { get; set; }
    public required BaseType Type { get; set; }
    public Dictionary<string, Field> Parameters { get; set; } = new();
    public Dictionary<string, Field> Exceptions { get; set; } = new();
    public Dictionary<string, string> Attributes { get; set; } = new();

    public string Url()
    {
        return this.Identifier.Underscore().Dasherize().ToLower();
    }

    public string Method()
    {
        if (Attributes.TryGetValue("http.method", out var method))
        {
            return method.ToUpper();
        }

        return "POST";
    }

    public List<BaseType> AllTypes()
    {
        return this.Exceptions.Values.Select(e => e.Type).Concat([this.Type]).ToList();
    }
}