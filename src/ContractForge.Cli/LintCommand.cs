using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

using NetRpc.Core;
using NetRpc.Core.Types;

using Spectre.Console;
using Spectre.Console.Cli;

namespace ContractForge.Cli;

public class LintCommand : Command<LintCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [Description("The .thirft file you would like to lint")]
        [CommandOption("-e|--entry")]
        public string? EntryPoint { get; init; }

        [Description("Add a directory to the list of directories searched for include directives")]
        [CommandOption("-i|--include")]
        public string[]? IncludePaths { get; init; }
    }

    public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        if (settings.EntryPoint is null)
        {
            throw new ArgumentNullException("Entry point is required");
        }

        var definitionState = new DefinitionState();

        // Add default include path (entry point directory)
        var entryPointDir = Path.GetDirectoryName(Path.GetFullPath(settings.EntryPoint));
        if (entryPointDir != null)
        {
            definitionState.AddIncludePath(entryPointDir);
        }

        if (settings.IncludePaths != null)
        {
            foreach (var path in settings.IncludePaths)
            {
                definitionState.AddIncludePath(path);
            }
        }

        try
        {
            definitionState.Load(settings.EntryPoint);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]ERROR:[/] Failed to load entry point: {ex.Message}");
            return 1;
        }

        var errors = definitionState.Documents.SelectMany(d => d.Value.Errors).ToList();

        if (errors.Count > 0)
        {
            foreach (var error in errors)
            {
                // ToMsBuildFormat already includes the severity/id
                AnsiConsole.MarkupLine($"[red]ERROR:[/] {error.ToMsBuildFormat()}");
                AnsiConsole.WriteLine(error.ToConsoleOutput());
            }

            AnsiConsole.MarkupLine($"[red]Linting failed with {errors.Count} error(s).[/]");
            return 1;
        }

        AnsiConsole.MarkupLine("[green]No issues found.[/]");
        return 0;
    }
}
