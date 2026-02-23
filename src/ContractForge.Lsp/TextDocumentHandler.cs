using MediatR;

using ContractForge.Core;
using ContractForge.Core.Thrift;

using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;

namespace ContractForge.Lsp;

public class TextDocumentHandler : TextDocumentSyncHandlerBase
{
    private readonly DefinitionState _definitionState;
    private readonly ILanguageServerFacade _server;
    private readonly ThriftLoader _loader = new();

    private int _version = 0;

    private readonly TextDocumentSelector _textDocumentSelector = new TextDocumentSelector(
        new TextDocumentFilter
        {
            Pattern = "**/*.thrift"
        }
    );

    public TextDocumentSyncKind Change { get; } = TextDocumentSyncKind.Full;

    public TextDocumentHandler(ILanguageServerFacade server, DefinitionState definitionState)
    {
        _server = server;
        _definitionState = definitionState;
    }

    public override TextDocumentAttributes GetTextDocumentAttributes(DocumentUri uri)
    {
        if (IsThriftFile(uri))
        {
            return new TextDocumentAttributes(uri, "thrift");
        }
        return new TextDocumentAttributes(uri, "unknown");
    }

    public override async Task<Unit> Handle(DidOpenTextDocumentParams request, CancellationToken cancellationToken)
    {
        if (!IsThriftFile(request.TextDocument.Uri)) return Unit.Value;

        try
        {
            var filesystemPath = DocumentUri.GetFileSystemPath(request.TextDocument.Uri);
            this.Update(filesystemPath ?? "", request.TextDocument.Text, request.TextDocument.Uri);
        }
        catch (Exception e)
        {
            await Console.Error.WriteLineAsync("ERROR in did open " + e.Message);
        }

        return await Unit.Task;
    }

    public override async Task<Unit> Handle(DidChangeTextDocumentParams request, CancellationToken cancellationToken)
    {
        if (!IsThriftFile(request.TextDocument.Uri)) return Unit.Value;

        try
        {
            var filesystemPath = DocumentUri.GetFileSystemPath(request.TextDocument.Uri);

            // Full sync means ContentChanges[0].Text is the whole file
            // Assuming TextDocumentSyncKind.Full
            var content = request.ContentChanges.FirstOrDefault()?.Text ?? "";

            this.Update(filesystemPath ?? "", content, request.TextDocument.Uri);
        }
        catch (Exception e)
        {
            await Console.Error.WriteLineAsync("ERROR in did change " + e.Message);
        }

        return await Unit.Task;
    }

    public override async Task<Unit> Handle(DidSaveTextDocumentParams request, CancellationToken cancellationToken)
    {
        if (!IsThriftFile(request.TextDocument.Uri)) return Unit.Value;

        try
        {
            var filesystemPath = DocumentUri.GetFileSystemPath(request.TextDocument.Uri);
            // On Save, we might want to read from disk OR use the text provided if available.
            // But if we are tracking changes, we should already have the state.
            // DidSaveTextDocumentParams can include text if configured.

            var content = request.Text ?? File.ReadAllText(filesystemPath ?? "");
            this.Update(filesystemPath ?? "", content, request.TextDocument.Uri);
        }
        catch (Exception e)
        {
            await Console.Error.WriteLineAsync("ERROR in did save " + e.Message);
        }

        return await Unit.Task;
    }

    public override Task<Unit> Handle(DidCloseTextDocumentParams request, CancellationToken cancellationToken)
    {
        return Unit.Task;
    }

    protected override TextDocumentSyncRegistrationOptions CreateRegistrationOptions(TextSynchronizationCapability capability, ClientCapabilities clientCapabilities)
    {
        return new TextDocumentSyncRegistrationOptions()
        {
            DocumentSelector = _textDocumentSelector,
            Change = Change,
            Save = new SaveOptions() { IncludeText = true }
        };
    }

    private void Update(string filesystemPath, string content, DocumentUri uri)
    {
        var document = new ContractForge.Core.Types.Document
        {
            Uri = filesystemPath,
            Content = content
        };

        _definitionState.Documents[filesystemPath] = document;
        _loader.Load(document, _definitionState);

        var diagnostics = document.Errors.Select(error =>
        {
            return new Diagnostic()
            {
                Severity = DiagnosticSeverity.Error,
                Message = error.Message,
                Range = new OmniSharp.Extensions.LanguageServer.Protocol.Models.Range(
                    error.Point.Line - 1,
                    error.Point.Column,
                    error.Point.Line - 1,
                    error.Point.Column + error.Point.Length
                )
            };
        });

        _server.TextDocument.PublishDiagnostics(new PublishDiagnosticsParams()
        {
            Uri = uri,
            Version = _version++,
            Diagnostics = new Container<Diagnostic>(diagnostics)
        });
    }

    private bool IsThriftFile(DocumentUri uri)
    {
        var path = DocumentUri.GetFileSystemPath(uri);
        return path != null && Path.GetExtension(path).Equals(".thrift", StringComparison.OrdinalIgnoreCase);
    }
}