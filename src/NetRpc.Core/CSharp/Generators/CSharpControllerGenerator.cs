using System.Text;

using Inflector;

using NetRpc.Core.Types;

namespace NetRpc.Core.CSharp.Generators;

/// <summary>
/// Generates ASP.NET Core controller base classes from service definitions
/// </summary>
public class CSharpControllerGenerator : ICSharpGenerator
{
    private readonly CSharpTypeMapper _typeMapper;

    public CSharpControllerGenerator(CSharpTypeMapper typeMapper)
    {
        _typeMapper = typeMapper;
    }

    public void Generate(StringBuilder builder, Document document, List<Error> errors, int indent)
    {
        var prefix = new string(' ', indent);

        foreach (var service in document.Services.Values)
        {
            builder.AppendFormat("{0}[Route(\"{1}\")]\n", prefix, service.Url());
            AppendAuthAttributes(builder, service.Attributes, prefix);
            CSharpDocumentationHelper.AppendXmlDocComment(builder, service.Description, indent);
            builder.AppendFormat("{0}public abstract class {1}BaseController : Controller\n", prefix, service.Identifier);
            builder.AppendFormat("{0}{{\n", prefix);

            builder.AppendFormat("{0}    private readonly I{1}Service _service;\n", prefix, service.Identifier);

            builder.AppendFormat("{0}    public {1}BaseController(I{1}Service service)\n", prefix, service.Identifier);
            builder.AppendFormat("{0}    {{\n", prefix);
            builder.AppendFormat("{0}        _service = service;\n", prefix);
            builder.AppendFormat("{0}    }}\n", prefix);

            foreach (var func in service.Functions.Values)
            {
                // Validate POST method parameters
                if (func.Method().ToUpper() == "POST" && func.Parameters.Count > 1)
                {
                    errors.Add(new Error(
                        document,
                        func.Point,
                        $"POST method '{func.Identifier}' has {func.Parameters.Count} parameters. " +
                        $"POST methods can only have one [FromBody] parameter. " +
                        $"Wrap multiple parameters in a struct instead."
                    ));
                    continue; // Skip generating this method
                }

                builder.AppendFormat("{0}    [Http{1}]\n", prefix, func.Method().ToLower().Pascalize());
                builder.AppendFormat("{0}    [Route(\"{1}\")]\n", prefix, func.Url());
                AppendAuthAttributes(builder, func.Attributes, prefix + "    ");
                CSharpDocumentationHelper.AppendXmlDocComment(builder, func.Description, indent + 4);
                builder.AppendFormat("{0}    public virtual {1} {2}(", prefix, _typeMapper.ToAsyncReturnType(func.Type), func.Identifier.Pascalize());

                var parameters = func.Parameters.Values.Select((parameter) =>
                {
                    var from = func.Method() == "POST" ? "[FromBody]" : "[FromQuery]";
                    return $"{from} {_typeMapper.ToCSharpType(parameter.Type)} {parameter.Identifier}";
                });

                builder.AppendJoin(", ", parameters);

                // Add CancellationToken as last parameter (ASP.NET auto-binds it)
                if (func.Parameters.Count > 0)
                {
                    builder.Append(", ");
                }
                builder.Append("CancellationToken cancellationToken = default");

                builder.Append(")\n");

                builder.AppendFormat("{0}    {{\n", prefix);

                // Handle void return type specially
                if (func.Type is Primitive p && p.Type == "void")
                {
                    builder.AppendFormat("{0}        await _service.{1}(", prefix, func.Identifier.Pascalize());

                    var allParams = func.Parameters.Values.Select((parameter) => parameter.Identifier).Concat(new[] { "cancellationToken" });
                    builder.AppendJoin(", ", allParams);

                    builder.Append(");\n");
                }
                else
                {
                    builder.AppendFormat("{0}        return _service.{1}(", prefix, func.Identifier.Pascalize());

                    var allParams = func.Parameters.Values.Select((parameter) => parameter.Identifier).Concat(new[] { "cancellationToken" });
                    builder.AppendJoin(", ", allParams);

                    builder.Append(");\n");
                }

                builder.AppendFormat("{0}    }}\n", prefix);
            }

            builder.AppendFormat("{0}}}\n", prefix);
        }
    }

    private void AppendAuthAttributes(StringBuilder builder, Dictionary<string, string> attributes, string prefix)
    {
        if (attributes.TryGetValue("authorize.policy", out var policy))
        {
            var policies = policy.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var p in policies)
            {
                builder.AppendFormat("{0}[Authorize(Policy = \"{1}\")]\n", prefix, p);
            }
        }
    }
}