using ContractForge.Core;
using ContractForge.Lsp.Abstractions;

using Microsoft.Extensions.DependencyInjection;

using OmniSharp.Extensions.LanguageServer.Server;

namespace ContractForge.Lsp;

public static class ContractForgeLsp
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