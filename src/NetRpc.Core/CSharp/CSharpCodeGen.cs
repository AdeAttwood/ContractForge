using System.Globalization;
using System.Text;

using Inflector;

using NetRpc.Core.CSharp.Generators;
using NetRpc.Core.Types;

namespace NetRpc.Core.CSharp;

/// <summary>
/// Orchestrates C# code generation using specialized generators
/// </summary>
public class CSharpCodeGen : ICodeGen
{
    private readonly CSharpTypeMapper _typeMapper;
    private readonly List<ICSharpGenerator> _generators;

    public CSharpCodeGen()
    {
        _typeMapper = new CSharpTypeMapper();

        _generators = new List<ICSharpGenerator>
        {
            new CSharpStructGenerator(_typeMapper),
            new CSharpEnumGenerator(),
            new CSharpExceptionGenerator(_typeMapper),
            new CSharpUnionGenerator(_typeMapper),
            new CSharpServiceInterfaceGenerator(_typeMapper),
            new CSharpControllerGenerator(_typeMapper)
        };
    }

    public CodeGenResult Build(DefinitionState state)
    {
        Inflector.Inflector.SetDefaultCultureFunc = () => new CultureInfo("en-GB");

        var builder = new StringBuilder();
        var errors = new List<Error>();

        foreach (var document in state.Documents.Values)
        {
            var ns = document.Namespaces.GetValueOrDefault("cs");
            var indent = 0;

            if (ns is not null)
            {
                builder.AppendFormat("namespace {0}\n{{\n", ns);
                indent = 4;
            }

            AppendUsings(builder, indent);

            foreach (var generator in _generators)
            {
                generator.Generate(builder, document, errors, indent);
            }

            if (ns is not null)
            {
                builder.Append("}\n");
            }
        }

        return new CodeGenResult
        {
            Output = builder.ToString(),
            Errors = errors
        };
    }

    private void AppendUsings(StringBuilder builder, int indent)
    {
        var prefix = new string(' ', indent);
        builder.AppendFormat("{0}using Microsoft.AspNetCore.Mvc;\n", prefix);
        builder.AppendFormat("{0}using System.Text.Json.Serialization;\n", prefix);
        builder.AppendFormat("{0}using System.Collections;\n", prefix);
        builder.AppendFormat("{0}using System.Threading;\n", prefix);
        builder.AppendFormat("{0}using System.Threading.Tasks;\n", prefix);
        builder.AppendFormat("{0}using System.ComponentModel.DataAnnotations;\n", prefix);
        builder.AppendFormat("{0}using Microsoft.AspNetCore.Authorization;\n", prefix);
    }
}