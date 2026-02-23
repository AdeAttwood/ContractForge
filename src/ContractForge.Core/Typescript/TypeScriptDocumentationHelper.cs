using System.Text;

namespace NetRpc.Core.Typescript;

/// <summary>
/// Handles generation of JSDoc comments for TypeScript code
/// </summary>
public static class TypeScriptDocumentationHelper
{
    /// <summary>
    /// Appends JSDoc comment with proper indentation
    /// </summary>
    /// <param name="builder">StringBuilder to append to</param>
    /// <param name="description">Description text (can be multi-line)</param>
    /// <param name="indent">Number of spaces to indent (0, 2, 4, etc.)</param>
    public static void AppendJsDocComment(StringBuilder builder, string? description, int indent)
    {
        if (string.IsNullOrWhiteSpace(description)) return;

        var prefix = new string(' ', indent);
        builder.AppendLine($"{prefix}/**");
        foreach (var line in description.Split('\n'))
        {
            builder.AppendLine($"{prefix} * {line}");
        }
        builder.AppendLine($"{prefix} */");
    }
}