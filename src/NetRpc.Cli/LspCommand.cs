using NetRpc.Lsp;

using Spectre.Console.Cli;

namespace NetRpc.Cli;

public class LspCommand : AsyncCommand<LspCommand.Settings>
{
    public class Settings : CommandSettings
    {
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var server = await NetRpcLsp.CreateLspAsync(Console.OpenStandardInput(), Console.OpenStandardOutput());

        await server.WaitForExit;
        return 0;
    }
}