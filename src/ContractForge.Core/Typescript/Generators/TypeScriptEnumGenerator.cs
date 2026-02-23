using System.Text;

using ContractForge.Core.Types;

namespace ContractForge.Core.Typescript.Generators;

/// <summary>
/// Generates TypeScript const objects and union types from enum definitions
/// </summary>
public class TypeScriptEnumGenerator : ITypeScriptGenerator
{
    private readonly TypeScriptTypeMapper _typeMapper;

    public TypeScriptEnumGenerator(TypeScriptTypeMapper typeMapper)
    {
        _typeMapper = typeMapper;
    }

    public void Generate(StringBuilder builder, Document document, List<Error> errors)
    {
        foreach (var enumType in document.Enums.Values)
        {
            var enumName = _typeMapper.ToPascalIdentifier(enumType.Identifier);

            TypeScriptDocumentationHelper.AppendJsDocComment(builder, enumType.Description, 0);
            builder.AppendFormat("export const {0} = {{\n", enumName);

            foreach (var value in enumType.Values.Values)
            {
                TypeScriptDocumentationHelper.AppendJsDocComment(builder, value.Description, 2);
                builder.AppendFormat("  {0}: \"{1}\",\n", _typeMapper.ToTypeScriptEnumKey(value), _typeMapper.ToTypeScriptEnumValue(value));
            }

            builder.Append("} as const;\n");
            builder.AppendFormat("export type {0} = typeof {0}[keyof typeof {0}];\n", enumName);
        }
    }
}