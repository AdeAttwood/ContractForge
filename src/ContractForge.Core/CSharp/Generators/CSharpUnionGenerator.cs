using System.Text;

using ContractForge.Core.Types;

using Inflector;

namespace ContractForge.Core.CSharp.Generators;

/// <summary>
/// Generates C# discriminated union types from union definitions
/// </summary>
public class CSharpUnionGenerator : ICSharpGenerator
{
    private readonly CSharpTypeMapper _typeMapper;

    public CSharpUnionGenerator(CSharpTypeMapper typeMapper)
    {
        _typeMapper = typeMapper;
    }

    public void Generate(StringBuilder builder, Document document, List<Error> errors, int indent)
    {
        var prefix = new string(' ', indent);

        foreach (var union in document.Unions.Values)
        {
            // Generate the discriminator enum
            builder.AppendFormat("{0}[JsonConverter(typeof(JsonStringEnumConverter))]\n", prefix);
            builder.AppendFormat("{0}public enum {1}Type\n", prefix, union.Identifier);
            builder.AppendFormat("{0}{{\n", prefix);

            foreach (var field in union.Fields.Values)
            {
                builder.AppendFormat("{0}    [JsonStringEnumMemberName(\"{1}\")]\n", prefix, field.Identifier);
                builder.AppendFormat("{0}    {1},\n", prefix, field.Identifier.Pascalize());
            }

            builder.AppendFormat("{0}}}\n", prefix);

            // Generate the union class
            CSharpDocumentationHelper.AppendXmlDocComment(builder, union.Description, indent);
            builder.AppendFormat("{0}public class {1}\n", prefix, union.Identifier);
            builder.AppendFormat("{0}{{\n", prefix);

            builder.AppendFormat("{0}    [JsonPropertyName(\"type\")]\n", prefix);
            builder.AppendFormat("{0}    public {1}Type Type {{ get; set; }}\n\n", prefix, union.Identifier);

            foreach (var field in union.Fields.Values)
            {
                CSharpDocumentationHelper.AppendXmlDocComment(builder, field.Description, indent + 4);
                builder.AppendFormat("{0}    [JsonPropertyName(\"{1}\")]\n", prefix, field.Identifier);
                builder.AppendFormat("{0}    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]\n", prefix);
                builder.AppendFormat("{0}    public {1}? {2} {{ get; set; }}\n", prefix, _typeMapper.ToCSharpType(field.Type), field.Identifier.Pascalize());
                builder.AppendFormat("{0}    public {1}({2} {3})\n", prefix, union.Identifier, _typeMapper.ToCSharpType(field.Type), field.Identifier);
                builder.AppendFormat("{0}    {{\n", prefix);
                builder.AppendFormat("{0}        Type = {1}Type.{2};\n", prefix, union.Identifier, field.Identifier.Pascalize());
                builder.AppendFormat("{0}        this.{1} = {2};\n", prefix, field.Identifier.Pascalize(), field.Identifier);
                builder.AppendFormat("{0}    }}\n", prefix);
            }

            builder.AppendFormat("{0}}}\n", prefix);
        }
    }
}