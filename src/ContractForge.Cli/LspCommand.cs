using ContractForge.Lsp;

using Spectre.Console.Cli;

namespace ContractForge.Cli;

public class LspCommand : AsyncCommand<LspCommand.Settings>
{
    public class Settings : CommandSettings
    {
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var server = await ContractForgeLsp.CreateLspAsync(Console.OpenStandardInput(), Console.OpenStandardOutput());

        await server.WaitForExit;
        return 0;
    }
}