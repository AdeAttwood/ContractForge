using System.Text;

using ContractForge.Core.CSharp;
using ContractForge.Core.Types;

using Inflector;

namespace ContractForge.Core.CSharp.Generators;

/// <summary>
/// Generates C# DTO classes from struct definitions
/// </summary>
public class CSharpStructGenerator : ICSharpGenerator
{
    private readonly CSharpTypeMapper _typeMapper;
    private readonly CSharpCodeGenOptions _options;

    public CSharpStructGenerator(CSharpTypeMapper typeMapper, CSharpCodeGenOptions options)
    {
        _typeMapper = typeMapper;
        _options = options;
    }

    public void Generate(StringBuilder builder, Document document, List<Error> errors, int indent)
    {
        var prefix = new string(' ', indent);

        foreach (var structType in document.Structs.Values)
        {
            CSharpDocumentationHelper.AppendXmlDocComment(builder, structType.Description, indent);
            var classModifier = _options.UsePartialClasses ? "partial " : "";
            builder.AppendFormat("{0}public {1}class {2}\n", prefix, classModifier, structType.Identifier);
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