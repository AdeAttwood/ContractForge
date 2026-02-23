using System.Text;

using ContractForge.Core.Types;

namespace ContractForge.Core.Typescript.Generators;

/// <summary>
/// Interface for TypeScript code generators that handle specific type categories
/// </summary>
public interface ITypeScriptGenerator
{
    /// <summary>
    /// Generates TypeScript code for the given document
    /// </summary>
    /// <param name="builder">StringBuilder to append generated code to</param>
    /// <param name="document">Document containing type definitions</param>
    /// <param name="errors">List to add any generation errors to</param>
    void Generate(StringBuilder builder, Document document, List<Error> errors);
}