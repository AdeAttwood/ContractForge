using ContractForge.Core;

using MediatR;

using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace ContractForge.Lsp;

public class DiagnosticsHandler : PublishDiagnosticsHandlerBase
{
    private readonly DefinitionState _definitionState;

    public DiagnosticsHandler(DefinitionState definitionState)
    {
        _definitionState = definitionState;
    }

    public override async Task<Unit> Handle(PublishDiagnosticsParams request, CancellationToken cancellationToken)
    {
        await Console.Error.WriteLineAsync($"We are diag on '{request.Uri.ToString()}'");

        foreach (var doc in _definitionState.Documents)
        {
            await Console.Error.WriteLineAsync($"Loaded '{doc.Key}'");
        }

        return await Unit.Task;
    }
}