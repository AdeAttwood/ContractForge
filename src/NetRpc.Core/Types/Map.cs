namespace NetRpc.Core.Types;

public class Map : BaseType
{
    public required BaseType Key { get; set; }
    public required BaseType Value { get; set; }
}