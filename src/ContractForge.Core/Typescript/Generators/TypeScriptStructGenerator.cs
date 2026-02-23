using System.Text;

using ContractForge.Core.Types;

using Inflector;

namespace ContractForge.Core.Typescript.Generators;

/// <summary>
/// Generates TypeScript interfaces from struct definitions
/// </summary>
public class TypeScriptStructGenerator : ITypeScriptGenerator
{
    private readonly TypeScriptTypeMapper _typeMapper;

    public TypeScriptStructGenerator(TypeScriptTypeMapper typeMapper)
    {
        _typeMapper = typeMapper;
    }

    public void Generate(StringBuilder builder, Document document, List<Error> errors)
    {
        foreach (var structType in document.Structs.Values)
        {
            TypeScriptDocumentationHelper.AppendJsDocComment(builder, structType.Description, 0);
            builder.AppendFormat("export interface {0} {{\n", structType.Identifier.Pascalize());

            foreach (var field in structType.Fields.Values)
            {
                TypeScriptDocumentationHelper.AppendJsDocComment(builder, field.Description, 2);
                builder.AppendFormat("  {0}: {1};\n", field.Identifier, _typeMapper.ToTypeScriptType(field.Type));
            }

            builder.Append("}\n");
        }
    }
}