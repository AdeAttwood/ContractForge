using Antlr4.Runtime;

using NetRpc.Core;
using NetRpc.Core.Types;
using NetRpc.Lsp.Abstractions;

using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace NetRpc.Lsp;

public class HoverHandler : HoverHandlerBase
{
    private readonly DefinitionState _definitionState;
    private readonly ITokenAnalyzer _tokenAnalyzer;

    public HoverHandler(DefinitionState definitionState, ITokenAnalyzer tokenAnalyzer)
    {
        _definitionState = definitionState;
        _tokenAnalyzer = tokenAnalyzer;
    }

    public override Task<Hover?> Handle(HoverParams request, CancellationToken cancellationToken)
    {
        var filesystemPath = DocumentUri.GetFileSystemPath(request.TextDocument);
        if (filesystemPath == null || !_definitionState.Documents.TryGetValue(filesystemPath, out var document))
        {
            return Task.FromResult<Hover?>(null);
        }

        // Find token at cursor
        var token = _tokenAnalyzer.GetTokenAtPosition(document, request.Position);
        if (token == null) return Task.FromResult<Hover?>(null);

        // We only care about Identifiers
        if (!IsIdentifier(token)) return Task.FromResult<Hover?>(null);

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

        // 1. Try resolving as Type (Struct, Enum, etc.) via Document.Resolve
        // This handles "Type" and "Alias.Type"
        var resolvedType = document.Resolve(lookup);
        if (resolvedType != null)
        {
            return Task.FromResult<Hover?>(CreateHover(resolvedType));
        }

        // 2. Try resolving as Service (Resolve doesn't cover Services currently)
        // Services are usually top-level, so strict lookup. 
        // But if we had "Alias.Service", we might need logic. 
        // For now, assume local service lookup or "Alias.Service" manually?
        // Document.Resolve logic for alias is: resolve alias doc, then call Resolve on it.
        // I can mimic that for services if needed.
        if (lookup.Contains("."))
        {
            var parts = lookup.Split('.', 2);
            if (document.IncludedDocuments.TryGetValue(parts[0], out var incDoc))
            {
                if (incDoc.Services.TryGetValue(parts[1], out var service))
                {
                    return Task.FromResult<Hover?>(CreateHover(service));
                }
            }
        }
        else
        {
            if (document.Services.TryGetValue(lookup, out var service))
            {
                return Task.FromResult<Hover?>(CreateHover(service));
            }
        }

        // 3. Try resolving as Include Alias
        if (!lookup.Contains(".") && document.IncludedDocuments.TryGetValue(lookup, out var includedDoc))
        {
            return Task.FromResult<Hover?>(new Hover
            {
                Contents = new MarkedStringsOrMarkupContent(new MarkupContent
                {
                    Kind = MarkupKind.Markdown,
                    Value = $"**Module** `{lookup}`\n\npath: *{includedDoc.Uri}*"
                })
            });
        }

        return Task.FromResult<Hover?>(null);
    }

    protected override HoverRegistrationOptions CreateRegistrationOptions(HoverCapability capability, ClientCapabilities clientCapabilities)
    {
        return new HoverRegistrationOptions
        {
            DocumentSelector = new TextDocumentSelector(
                new TextDocumentFilter
                {
                    Pattern = "**/*.thrift"
                }
            )
        };
    }

    private Hover CreateHover(BaseType type)
    {
        string title = "Type";
        string name = "Unknown";
        string? description = null;

        switch (type)
        {
            case ExceptionStruct ex:
                title = "exception";
                name = ex.Identifier;
                description = ex.Description;
                break;
            case Struct s:
                title = "struct";
                name = s.Identifier;
                description = s.Description;
                break;
            case Service s:
                title = "service";
                name = s.Identifier;
                description = s.Description;
                break;
            case NetRpc.Core.Types.Enum e:
                title = "enum";
                name = e.Identifier;
                description = e.Description;
                break;
            case Union u:
                title = "union";
                name = u.Identifier;
                description = u.Description;
                break;
        }

        var markdown = $"```thrift\n{title} {name}\n```";
        if (!string.IsNullOrWhiteSpace(description))
        {
            markdown += $"\n---\n{description}";
        }

        return new Hover
        {
            Contents = new MarkedStringsOrMarkupContent(new MarkupContent
            {
                Kind = MarkupKind.Markdown,
                Value = markdown
            })
        };
    }

    private bool IsIdentifier(IToken token)
    {
        // Basic check: starts with letter/underscore. 
        // Could check token type if I had access to ThriftLexer.ID const, 
        // but text check is robust enough for hover.
        var text = token.Text;
        if (string.IsNullOrEmpty(text)) return false;
        char first = text[0];
        return char.IsLetter(first) || first == '_';
    }
}