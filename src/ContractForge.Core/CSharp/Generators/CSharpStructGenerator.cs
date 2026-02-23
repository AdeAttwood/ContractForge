using System.Text;

using Inflector;

using ContractForge.Core.Types;

namespace ContractForge.Core.CSharp.Generators;

/// <summary>
/// Generates C# DTO classes from struct definitions
/// </summary>
public class CSharpStructGenerator : ICSharpGenerator
{
    private readonly CSharpTypeMapper _typeMapper;

    public CSharpStructGenerator(CSharpTypeMapper typeMapper)
    {
        _typeMapper = typeMapper;
    }

    public void Generate(StringBuilder builder, Document document, List<Error> errors, int indent)
    {
        var prefix = new string(' ', indent);

        foreach (var structType in document.Structs.Values)
        {
            CSharpDocumentationHelper.AppendXmlDocComment(builder, structType.Description, indent);
            builder.AppendFormat("{0}public class {1}\n", prefix, structType.Identifier);
            builder.AppendFormat("{0}{{\n", prefix);

            foreach (var field in structType.Fields.Values)
            {
                CSharpDocumentationHelper.AppendXmlDocComment(builder, field.Description, indent + 4);

                // Add [Required] attribute for fields with 'required' keyword
                if (field.IsRequired)
                {
                    builder.AppendFormat("{0}    [Required]\n", prefix);
                }

                builder.AppendFormat("{0}    [JsonPropertyName(\"{1}\")]\n", prefix, field.Identifier);

                // Generate property with appropriate nullability and required modifier
                // - 'required' keyword → [Required] + required + non-nullable
                // - No keyword or 'optional' → no [Required] + no required + nullable
                var csharpType = _typeMapper.ToCSharpType(field.Type);
                var propertyModifier = field.IsRequired ? "required" : "";
                var nullableType = field.IsRequired ? csharpType : $"{csharpType}?";

                builder.AppendFormat("{0}    public {1} {2} {3} {{ get; set; }}\n",
                    prefix,
                    propertyModifier,
                    nullableType,
                    field.Identifier.Pascalize());
            }

            builder.AppendFormat("{0}}}\n", prefix);
        }
    }
}