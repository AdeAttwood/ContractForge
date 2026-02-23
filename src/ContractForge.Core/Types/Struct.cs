namespace NetRpc.Core.Types;

public class Struct : BaseType
{
    public required string Identifier { get; set; }
    public string? Description { get; set; }
    public Dictionary<string, Field> Fields { get; set; } = new();
    public Dictionary<string, string> Attributes { get; set; } = new();
}