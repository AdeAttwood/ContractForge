using System.Text;

using Inflector;

using ContractForge.Core.Types;

namespace ContractForge.Core.CSharp.Generators;

/// <summary>
/// Generates C# enum types from enum definitions
/// </summary>
public class CSharpEnumGenerator : ICSharpGenerator
{
    public void Generate(StringBuilder builder, Document document, List<Error> errors, int indent)
    {
        var prefix = new string(' ', indent);

        foreach (var enumType in document.Enums.Values)
        {
            CSharpDocumentationHelper.AppendXmlDocComment(builder, enumType.Description, indent);
            builder.AppendFormat("{0}[JsonConverter(typeof(JsonStringEnumConverter))]\n", prefix);
            builder.AppendFormat("{0}public enum {1}\n", prefix, enumType.Identifier);
            builder.AppendFormat("{0}{{\n", prefix);

            foreach (var value in enumType.Values.Values)
            {
                CSharpDocumentationHelper.AppendXmlDocComment(builder, value.Description, indent + 4);
                // Convert ALL_CAPS names to PascalCase (PENDING -> Pending)
                var pascalName = value.Name.ToLower().Pascalize();
                builder.AppendFormat("{0}    {1} = {2},\n", prefix, pascalName, value.Value);
            }

            builder.AppendFormat("{0}}}\n", prefix);
        }
    }
}