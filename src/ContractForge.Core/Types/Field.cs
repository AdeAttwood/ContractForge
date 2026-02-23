namespace NetRpc.Core.Types;

public class Field : BaseType
{
    public required int Index { get; set; }

    public required BaseType Type { get; set; }

    public required string Identifier { get; set; }

    public string? Description { get; set; }

    /// <summary>
    /// Indicates if the field has the 'required' keyword in Thrift IDL.
    /// If true, generates [Required] attribute and non-nullable type.
    /// If false (default or 'optional' keyword), generates nullable type without [Required].
    /// </summary>
    public required bool IsRequired { get; set; }

    public Dictionary<string, string> Attributes { get; set; } = new();
}