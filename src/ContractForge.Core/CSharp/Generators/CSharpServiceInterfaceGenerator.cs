using System.Text;

using Inflector;

using ContractForge.Core.Types;

namespace ContractForge.Core.CSharp.Generators;

/// <summary>
/// Generates C# service interfaces from service definitions
/// </summary>
public class CSharpServiceInterfaceGenerator : ICSharpGenerator
{
    private readonly CSharpTypeMapper _typeMapper;

    public CSharpServiceInterfaceGenerator(CSharpTypeMapper typeMapper)
    {
        _typeMapper = typeMapper;
    }

    public void Generate(StringBuilder builder, Document document, List<Error> errors, int indent)
    {
        var prefix = new string(' ', indent);

        foreach (var service in document.Services.Values)
        {
            CSharpDocumentationHelper.AppendXmlDocComment(builder, service.Description, indent);
            builder.AppendFormat("{0}public interface I{1}Service\n", prefix, service.Identifier);
            builder.AppendFormat("{0}{{\n", prefix);

            foreach (var func in service.Functions.Values)
            {
                CSharpDocumentationHelper.AppendXmlDocComment(builder, func.Description, indent + 4);
                builder.AppendFormat("{0}    {1} {2}(", prefix, _typeMapper.ToAsyncReturnType(func.Type), func.Identifier.Pascalize());

                var parameters = func.Parameters.Values.Select((parameter) =>
                {
                    return $"{_typeMapper.ToCSharpType(parameter.Type)} {parameter.Identifier}";
                });

                builder.AppendJoin(", ", parameters);

                // Add CancellationToken as last parameter
                if (func.Parameters.Count > 0)
                {
                    builder.Append(", ");
                }
                builder.Append("CancellationToken cancellationToken = default");

                builder.Append(");\n");
            }

            builder.AppendFormat("{0}}}\n", prefix);
        }
    }
}