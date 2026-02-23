using Antlr4.Runtime;

using ContractForge.Core;
using ContractForge.Core.Types;
using ContractForge.Lsp.Abstractions;

using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace ContractForge.Lsp;

public class DefinitionHandler : DefinitionHandlerBase
{
    private readonly DefinitionState _definitionState;
    private readonly ITokenAnalyzer _tokenAnalyzer;

    public DefinitionHandler(DefinitionState definitionState, ITokenAnalyzer tokenAnalyzer)
    {
        _definitionState = definitionState;
        _tokenAnalyzer = tokenAnalyzer;
    }

    public override Task<LocationOrLocationLinks?> Handle(DefinitionParams request, CancellationToken cancellationToken)
    {
        var filesystemPath = DocumentUri.GetFileSystemPath(request.TextDocument);
        if (filesystemPath == null || !_definitionState.Documents.TryGetValue(filesystemPath, out var document))
        {
            return Task.FromResult<LocationOrLocationLinks?>(new LocationOrLocationLinks());
        }

        // Find token at cursor
        var token = _tokenAnalyzer.GetTokenAtPosition(document, request.Position);
        if (token == null || !IsIdentifier(token)) return Task.FromResult<LocationOrLocationLinks?>(new LocationOrLocationLinks());

        string lookup = token.Text;

        // Check for "Alias.Type" scenario (Look behind)
        var tokens = document.Tokens;
        var tokenIndex = tokens.IndexOf(token);
        if (tokenIndex > 1)
        {
            var prev = tokens[tokenIndex - 1];
            var prevPrev = tokens[tokenIndex - 2];
            if (prev.Text == "." && IsIdentifier(prevPrev))
            {
                lookup = $"{prevPrev.Text}.{token.Text}";
            }
        }

        // 1. Try resolving as Type (Struct, Enum, etc.)
        var resolvedType = document.Resolve(lookup);
        if (resolvedType != null)
        {
            return Task.FromResult<LocationOrLocationLinks?>(CreateLocation(resolvedType.Document.Uri, resolvedType.Point));
        }

        // 2. Try resolving as Service
        if (lookup.Contains("."))
        {
            var parts = lookup.Split('.', 2);
            if (document.IncludedDocuments.TryGetValue(parts[0], out var incDoc))
            {
                if (incDoc.Services.TryGetValue(parts[1], out var service))
                {
                    return Task.FromResult<LocationOrLocationLinks?>(CreateLocation(incDoc.Uri, service.Point));
                }
            }
        }
        else
        {
            if (document.Services.TryGetValue(lookup, out var service))
            {
                return Task.FromResult<LocationOrLocationLinks?>(CreateLocation(document.Uri, service.Point));
            }
        }

        // 3. Try resolving as Include Alias (Go to the included file)
        // If the user clicks "Shared", we take them to the start of "Shared.thrift"? 
        // Or to the "include 'Shared.thrift'" line in the current file?
        // Usually Go To Definition on an alias takes you to the file.
        if (!lookup.Contains(".") && document.IncludedDocuments.TryGetValue(lookup, out var includedDoc))
        {
            // Point to start of file (0,0)
            return Task.FromResult<LocationOrLocationLinks?>(CreateLocation(includedDoc.Uri, new Point { Line = 1, Column = 0, Length = 0 }));
        }

        return Task.FromResult<LocationOrLocationLinks?>(new LocationOrLocationLinks());
    }

    protected override DefinitionRegistrationOptions CreateRegistrationOptions(DefinitionCapability capability, ClientCapabilities clientCapabilities)
    {
        return new DefinitionRegistrationOptions
        {
            DocumentSelector = new TextDocumentSelector(
                new TextDocumentFilter
                {
                    Pattern = "**/*.thrift"
                }
            )
        };
    }

    private LocationOrLocationLinks CreateLocation(string uri, Point point)
    {
        // ANTLR Line is 1-based, LSP is 0-based
        var startLine = point.Line - 1;
        var startCol = point.Column;
        var endLine = startLine;
        var endCol = startCol + point.Length;

        return new LocationOrLocationLinks(new Location
        {
            Uri = DocumentUri.From(uri),
            Range = new Range(startLine, startCol, endLine, endCol)
        });
    }

    private bool IsIdentifier(IToken token)
    {
        var text = token.Text;
        if (string.IsNullOrEmpty(text)) return false;
        char first = text[0];
        return char.IsLetter(first) || first == '_';
    }
}