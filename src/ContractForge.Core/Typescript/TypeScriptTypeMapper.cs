using ContractForge.Core.Types;

using Inflector;

namespace ContractForge.Core.Typescript;

/// <summary>
/// Handles conversion of ContractForge types to TypeScript type strings
/// </summary>
public class TypeScriptTypeMapper
{
    /// <summary>
    /// Converts a BaseType to its TypeScript type string representation
    /// </summary>
    public string ToTypeScriptType(BaseType type)
    {
        return type switch
        {
            Struct s => s.Identifier,
            Union u => u.Identifier,
            Types.Enum e => e.Identifier.Pascalize(),
            List l => $"Array<{ToTypeScriptType(l.InnerType)}>",
            Map m => $"Record<{ToTypeScriptType(m.Key)}, {ToTypeScriptType(m.Value)}>",
            Primitive p => p.Type switch
            {
                "i32" => "number",
                "i64" => "number",
                "double" => "number",
                "string" => "string",
                "bool" => "boolean",
                _ => throw new Exception($"Invalid primitive value '{p.Type}' in TypeScript codegen"),
            },
            _ => throw new Exception($"Unable to convert {type} to a TypeScript type"),
        };
    }

    public string ToTypeScriptEnumValue(EnumValue value)
    {
        var pascalCase = value.Name.Pascalize();
        return char.ToLowerInvariant(pascalCase[0]) + pascalCase[1..];
    }

    public string ToTypeScriptEnumKey(EnumValue value)
    {
        return value.Name.Underscore().ToUpperInvariant();
    }

    public string ToPascalIdentifier(string identifier)
    {
        return identifier.Pascalize();
    }
}