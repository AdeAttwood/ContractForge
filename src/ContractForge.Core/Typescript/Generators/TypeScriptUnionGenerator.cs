using System.Text;

using Inflector;

using NetRpc.Core.Types;

namespace NetRpc.Core.Typescript.Generators;

/// <summary>
/// Generates TypeScript discriminated union types from union definitions
/// </summary>
public class TypeScriptUnionGenerator : ITypeScriptGenerator
{
    private readonly TypeScriptTypeMapper _typeMapper;

    public TypeScriptUnionGenerator(TypeScriptTypeMapper typeMapper)
    {
        _typeMapper = typeMapper;
    }

    public void Generate(StringBuilder builder, Document document, List<Error> errors)
    {
        foreach (var union in document.Unions.Values)
        {
            TypeScriptDocumentationHelper.AppendJsDocComment(builder, union.Description, 0);
            builder.AppendFormat("export type {0} =\n", union.Identifier.Pascalize());

            foreach (var field in union.Fields.Values)
            {
                builder.AppendFormat("  | {{ type: \"{0}\", {1}: {2} }}\n",
                    field.Identifier,
                    field.Identifier,
                    _typeMapper.ToTypeScriptType(field.Type));
            }
        }
    }
}