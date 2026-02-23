namespace ContractForge.Core.Types;

public class List : BaseType
{
    public required BaseType InnerType { get; set; }
}