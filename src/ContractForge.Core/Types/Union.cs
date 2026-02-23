namespace NetRpc.Core.Types;

public class Union : BaseType
{
    public required string Identifier { get; set; }
    public string? Description { get; set; }
    public Dictionary<string, Field> Fields { get; set; } = new();
}