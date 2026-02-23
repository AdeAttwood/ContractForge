using System.Text;

using ContractForge.Core.Types;
using Inflector;

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
                var pascalName = value.Name.Pascalize();
                var serializedName = pascalName.Camelize();
                builder.AppendFormat("{0}    [JsonStringEnumMemberName(\"{1}\")]\n", prefix, serializedName);
                builder.AppendFormat("{0}    {1} = {2},\n", prefix, pascalName, value.Value);
            }

            builder.AppendFormat("{0}}}\n", prefix);
        }
    }

}
