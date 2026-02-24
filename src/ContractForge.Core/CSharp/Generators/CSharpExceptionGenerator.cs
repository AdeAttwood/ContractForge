using System.Text;

using ContractForge.Core.CSharp;
using ContractForge.Core.Types;

using Inflector;

namespace ContractForge.Core.CSharp.Generators;

/// <summary>
/// Generates C# exception classes from exception definitions
/// </summary>
public class CSharpExceptionGenerator : ICSharpGenerator
{
    private readonly CSharpTypeMapper _typeMapper;
    private readonly CSharpCodeGenOptions _options;

    public CSharpExceptionGenerator(CSharpTypeMapper typeMapper, CSharpCodeGenOptions options)
    {
        _typeMapper = typeMapper;
        _options = options;
    }

    public void Generate(StringBuilder builder, Document document, List<Error> errors, int indent)
    {
        var prefix = new string(' ', indent);

        foreach (var exceptionStructType in document.Exceptions.Values)
        {
            CSharpDocumentationHelper.AppendXmlDocComment(builder, exceptionStructType.Description, indent);
            var classModifier = _options.UsePartialClasses ? "partial " : "";
            builder.AppendFormat("{0}public {1}class {2} : Exception\n", prefix, classModifier, exceptionStructType.Identifier);
            builder.AppendFormat("{0}{{\n", prefix);

            foreach (var field in exceptionStructType.Fields.Values)
            {
                if (field.Identifier.Pascalize() == "Message")
                {
                    continue;
                }

                CSharpDocumentationHelper.AppendXmlDocComment(builder, field.Description, indent + 4);

                // Add [Required] attribute for fields with 'required' keyword
                if (field.IsRequired)
                {
                    builder.AppendFormat("{0}    [Required]\n", prefix);
                }

                builder.AppendFormat("{0}    [JsonPropertyName(\"{1}\")]\n", prefix, field.Identifier);

                // Generate property with appropriate nullability
                // - 'required' keyword → non-nullable
                // - No keyword or 'optional' → nullable
                var csharpType = _typeMapper.ToCSharpType(field.Type);
                var nullableType = field.IsRequired ? csharpType : $"{csharpType}?";

                builder.AppendFormat("{0}    public {1} {2} {{ get; set; }}\n", prefix, nullableType, field.Identifier.Pascalize());
            }

            var parameters = exceptionStructType.Fields.Values
                .Where(f => f.Identifier.Pascalize() != "Message")
                .Select((parameter) =>
                {
                    return $"{_typeMapper.ToCSharpType(parameter.Type)} {parameter.Identifier}";
                });

            builder.AppendFormat("{0}    public {1}(", prefix, exceptionStructType.Identifier);
            builder.AppendJoin(", ", parameters);
            builder.Append(", string message): base(message)\n");
            builder.AppendFormat("{0}    {{\n", prefix);

            foreach (var field in exceptionStructType.Fields.Values)
            {
                if (field.Identifier.Pascalize() == "Message")
                {
                    continue;
                }

                builder.AppendFormat("{0}        {1} = {2};\n", prefix, field.Identifier.Pascalize(), field.Identifier);
            }

            builder.AppendFormat("{0}    }}\n", prefix);
            builder.AppendFormat("{0}}}\n", prefix);
        }
    }
}