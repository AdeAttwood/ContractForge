using System.Text;

namespace NetRpc.Core.CSharp;

/// <summary>
/// Handles generation of XML documentation comments for C# code
/// </summary>
public static class CSharpDocumentationHelper
{
    /// <summary>
    /// Appends XML doc comment with proper indentation
    /// </summary>
    /// <param name="builder">StringBuilder to append to</param>
    /// <param name="description">Description text (can be multi-line)</param>
    /// <param name="indent">Number of spaces to indent (0, 4, 8, etc.)</param>
    public static void AppendXmlDocComment(StringBuilder builder, string? description, int indent)
    {
        if (string.IsNullOrWhiteSpace(description)) return;

        var prefix = new string(' ', indent);
        builder.AppendLine($"{prefix}/// <summary>");
        foreach (var line in description.Split('\n'))
        {
            builder.AppendLine($"{prefix}/// {line}");
        }
        builder.AppendLine($"{prefix}/// </summary>");
    }
}