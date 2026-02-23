using ContractForge.Core.Types;

namespace ContractForge.Core.CSharp;

/// <summary>
/// Handles conversion of ContractForge types to C# type strings
/// </summary>
public class CSharpTypeMapper
{
    /// <summary>
    /// Converts a BaseType to its C# type string representation
    /// </summary>
    public string ToCSharpType(BaseType type)
    {
        return type switch
        {
            Struct s => s.Identifier,
            Union u => u.Identifier,
            Types.Enum e => e.Identifier,
            List l => $"IEnumerable<{ToCSharpType(l.InnerType)}>",
            Map m => $"Dictionary<{ToCSharpType(m.Key)}, {ToCSharpType(m.Value)}>",
            Primitive p => p.Type switch
            {
                "i32" => "int",
                "i64" => "long",
                "string" => "string",
                "double" => "double",
                "bool" => "bool",
                "void" => "void",
                _ => throw new Exception($"Invalid primitive value '{p.Type}' in c# codegen"),
            },
            _ => throw new Exception($"Unable to convert {type} in to a c# type"),
        };
    }

    /// <summary>
    /// Converts a BaseType to an async return type (Task&lt;T&gt; or Task for void)
    /// </summary>
    public string ToAsyncReturnType(BaseType type)
    {
        var syncType = ToCSharpType(type).Replace("IEnumerable<", "IAsyncEnumerable<");

        // Special handling for void - becomes Task (not Task<void>)
        if (type is Primitive p && p.Type == "void")
        {
            return "Task";
        }

        return $"Task<{syncType}>";
    }

    /// <summary>
    /// Determines if a type is a value type (affects nullability)
    /// </summary>
    public bool IsValueType(BaseType type)
    {
        return type switch
        {
            // Primitives are value types (except string which is reference but handled as non-nullable)
            Primitive p => p.Type switch
            {
                "i32" => true,
                "i64" => true,
                "double" => true,
                "bool" => true,
                "void" => true,
                "string" => false, // string is reference type but we want to allow nullability
                _ => false,
            },
            // Enums are value types
            Types.Enum => true,
            // Structs, Unions, Lists, Maps are reference types
            _ => false,
        };
    }

    /// <summary>
    /// Gets the nullable version of a type based on required flag
    /// </summary>
    public string GetNullableType(BaseType type, bool isRequired)
    {
        var csharpType = ToCSharpType(type);
        return isRequired ? csharpType : $"{csharpType}?";
    }
}
