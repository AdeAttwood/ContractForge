using Microsoft.Extensions.DependencyInjection;

using NetRpc.Core;
using NetRpc.Lsp.Abstractions;

using OmniSharp.Extensions.LanguageServer.Server;

namespace NetRpc.Lsp;

public static class NetRpcLsp
{
    public static async Task<LanguageServer> CreateLspAsync(Stream input, Stream output)
    {
        return await LanguageServer.From(options =>
        {
            options
                .WithInput(input)
                .WithOutput(output)
                .WithHandler<TextDocumentHandler>()
                .WithHandler<DocumentHighlightHandler>()
                .WithHandler<DiagnosticsHandler>()
                .WithHandler<CompletionHandler>()
                .WithHandler<HoverHandler>()
                .WithHandler<DefinitionHandler>()
                .WithHandler<ReferenceHandler>();

            options.WithServices(services =>
            {
                services.AddSingleton(new DefinitionState());
                services.AddSingleton<ITokenAnalyzer, TokenAnalyzer>();
            });

            options.OnInitialize(async (server, request, token) =>
            {
                var definitionState = server.GetRequiredService<DefinitionState>();

                await Console.Error.WriteLineAsync($"Initializing");
                foreach (var folder in request.WorkspaceFolders ?? [])
                {
                    await Console.Error.WriteLineAsync($"Initializing folder '{folder.Uri}'");
                }
            });
        });
    }
}