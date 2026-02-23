using System.Text;

using ContractForge.Core.Types;

namespace ContractForge.Core.CSharp.Generators;

/// <summary>
/// Interface for C# code generators that handle specific type categories
/// </summary>
public interface ICSharpGenerator
{
    /// <summary>
    /// Generates C# code for the given document
    /// </summary>
    /// <param name="builder">StringBuilder to append generated code to</param>
    /// <param name="document">Document containing type definitions</param>
    /// <param name="errors">List to add any generation errors to</param>
    /// <param name="indent">Base indentation level (0 for no namespace, 4 for inside namespace)</param>
    void Generate(StringBuilder builder, Document document, List<Error> errors, int indent);
}