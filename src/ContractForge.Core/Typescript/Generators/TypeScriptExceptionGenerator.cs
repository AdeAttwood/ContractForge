using System.Text;

using Inflector;

using ContractForge.Core.Types;

namespace ContractForge.Core.Typescript.Generators;

/// <summary>
/// Generates TypeScript interfaces from exception definitions
/// </summary>
public class TypeScriptExceptionGenerator : ITypeScriptGenerator
{
    private readonly TypeScriptTypeMapper _typeMapper;

    public TypeScriptExceptionGenerator(TypeScriptTypeMapper typeMapper)
    {
        _typeMapper = typeMapper;
    }

    public void Generate(StringBuilder builder, Document document, List<Error> errors)
    {
        foreach (var exceptionType in document.Exceptions.Values)
        {
            TypeScriptDocumentationHelper.AppendJsDocComment(builder, exceptionType.Description, 0);
            builder.AppendFormat("export interface {0} {{\n", exceptionType.Identifier.Pascalize());

            foreach (var field in exceptionType.Fields.Values)
            {
                TypeScriptDocumentationHelper.AppendJsDocComment(builder, field.Description, 2);
                builder.AppendFormat("  {0}: {1};\n", field.Identifier, _typeMapper.ToTypeScriptType(field.Type));
            }

            builder.Append("}\n");
        }
    }
}