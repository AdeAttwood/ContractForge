using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

using ContractForge.Core;
using ContractForge.Core.CSharp;
using ContractForge.Core.Typescript;

using Spectre.Console;
using Spectre.Console.Cli;

namespace ContractForge.Cli;

public class CodeGenCommand : Command<CodeGenCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [Description("The .thrift file(s) you would like to use as the entry point(s) to your service definition. Can be specified multiple times.")]
        [CommandOption("-e|--entry")]
        public string[]? EntryPoints { get; init; }

        [Description("The output code generator you would like to use")]
        [CommandOption("-g|--generator")]
        public string? Generator { get; init; }

        [Description("The output file to save the generated code to (optional, defaults to console)")]
        [CommandOption("-o|--output")]
        public string? OutputFile { get; init; }

        [Description("Generator option in key=value format. Can be specified multiple times.")]
        [CommandOption("-O|--option")]
        public string[]? Options { get; init; }

        [Description("Add a directory to the list of directories searched for include directives")]
        [CommandOption("-i|--include")]
        public string[]? IncludePaths { get; init; }
    }

    public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        if (settings.EntryPoints is null || settings.EntryPoints.Length == 0)
        {
            throw new ArgumentNullException("Entry point is required");
        }

        var definitionState = new DefinitionState();

        // Add default include paths from all entry point directories
        foreach (var entryPoint in settings.EntryPoints)
        {
            var entryPointDir = Path.GetDirectoryName(Path.GetFullPath(entryPoint));
            if (entryPointDir != null)
            {
                definitionState.AddIncludePath(entryPointDir);
            }
        }

        if (settings.IncludePaths != null)
        {
            foreach (var path in settings.IncludePaths)
            {
                definitionState.AddIncludePath(path);
            }
        }

        // Load all entry points
        foreach (var entryPoint in settings.EntryPoints)
        {
            definitionState.Load(entryPoint);
        }
        var errors = definitionState.Documents.SelectMany(d => d.Value.Errors);
        if (errors.Count() > 0)
        {
            foreach (var error in errors)
            {
                AnsiConsole.MarkupLine($"[red]ERROR:[/] {error.ToMsBuildFormat()}");
                AnsiConsole.WriteLine(error.ToConsoleOutput());
            }

            return 1;
        }

        ICodeGen codeGen = settings.Generator switch
        {
            "csharp-jsonapi" => new CSharpCodeGen(new CSharpCodeGenOptions(ParseOptions(settings.Options))),
            "typescript-client" => new TypescriptCodeGen(),
            _ => throw new ArgumentException($"Invalid generator '{settings.Generator}'"),
        };

        var result = codeGen.Build(definitionState);

        if (result.Errors.Count() > 0)
        {
            foreach (var error in result.Errors)
            {
                AnsiConsole.MarkupLine($"[red]ERROR:[/] {error.ToMsBuildFormat()}");
                AnsiConsole.WriteLine(error.ToConsoleOutput());
            }

            return 1;
        }

        if (settings.OutputFile is not null)
        {
            File.WriteAllText(settings.OutputFile, result.Output);
        }
        else
        {
            Console.WriteLine(result.Output);
        }

        return 0;
    }

    private static Dictionary<string, string> ParseOptions(string[]? options)
    {
        var parsedOptions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (options is null || options.Length == 0)
        {
            return parsedOptions;
        }

        foreach (var option in options)
        {
            var parts = option.Split('=', 2, StringSplitOptions.TrimEntries);
            if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[0]))
            {
                throw new ArgumentException($"Invalid option '{option}'. Expected key=value format.");
            }

            var key = parts[0];
            var value = parts[1];

            parsedOptions[key] = value;
        }

        return parsedOptions;
    }
}