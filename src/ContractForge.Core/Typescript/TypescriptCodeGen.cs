using System.Globalization;
using System.Text;

using ContractForge.Core.Types;
using ContractForge.Core.Typescript.Generators;

using Inflector;

namespace ContractForge.Core.Typescript;

/// <summary>
/// Orchestrates TypeScript code generation using specialized generators
/// </summary>
public class TypescriptCodeGen : ICodeGen
{
    private readonly TypeScriptTypeMapper _typeMapper;
    private readonly List<ITypeScriptGenerator> _generators;

    public TypescriptCodeGen()
    {
        _typeMapper = new TypeScriptTypeMapper();

        _generators = new List<ITypeScriptGenerator>
        {
            new TypeScriptEnumGenerator(_typeMapper),
            new TypeScriptStructGenerator(_typeMapper),
            new TypeScriptExceptionGenerator(_typeMapper),
            new TypeScriptUnionGenerator(_typeMapper),
            new TypeScriptClientGenerator(_typeMapper)
        };
    }

    public CodeGenResult Build(DefinitionState state)
    {
        Inflector.Inflector.SetDefaultCultureFunc = () => new CultureInfo("en-GB");

        var builder = new StringBuilder();
        var errors = new List<Error>();

        // Append utility functions
        builder.Append("""
        type Primitive = string | number | boolean | bigint | symbol | null | undefined;

        export function flatten<T extends Record<string, unknown>>(
          obj: T,
          parentKey = "",
        ): Record<string, Primitive> {
          const result: Record<string, Primitive> = {};

          for (const key in obj) {
            if (!Object.hasOwn(obj, key)) continue;

            const value = obj[key];
            const newKey = parentKey ? `${parentKey}.${key}` : key;

            if (
              typeof value === "object" &&
              value !== null &&
              !Array.isArray(value)
            ) {
              Object.assign(result, flatten(value as Record<string, unknown>, newKey));
            } else {
              result[newKey] = value as Primitive;
            }
          }

          return result;
        }


        """);

        foreach (var document in state.Documents.Values)
        {
            foreach (var generator in _generators)
            {
                generator.Generate(builder, document, errors);
            }
        }

        return new CodeGenResult
        {
            Output = builder.ToString(),
            Errors = errors
        };
    }
}