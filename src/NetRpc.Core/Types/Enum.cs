namespace NetRpc.Core.Types;

public class Enum : BaseType
{
    public required string Identifier { get; set; }
    public string? Description { get; set; }

    // Dictionary to preserve order and store enum values
    public Dictionary<string, EnumValue> Values { get; set; } = new();
    public Dictionary<string, string> Attributes { get; set; } = new();
}

public class EnumValue
{
    public required string Name { get; set; }
    public required int Value { get; set; }
    public string? Description { get; set; }
}