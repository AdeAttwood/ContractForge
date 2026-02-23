using Antlr4.Runtime;

using ContractForge.Core;
using ContractForge.Core.Types;
using ContractForge.Lsp.Abstractions;
using ContractForge.ThriftParser;

using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;

using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace ContractForge.Lsp;

public class CompletionHandler : CompletionHandlerBase
{
    private readonly DefinitionState _definitionState;
    private readonly ITokenAnalyzer _tokenAnalyzer;

    public CompletionHandler(DefinitionState definitionState, ITokenAnalyzer tokenAnalyzer)
    {
        _definitionState = definitionState;
        _tokenAnalyzer = tokenAnalyzer;
    }

    public override Task<CompletionList> Handle(CompletionParams request, CancellationToken cancellationToken)
    {
        var filesystemPath = DocumentUri.GetFileSystemPath(request.TextDocument);
        if (filesystemPath == null || !_definitionState.Documents.TryGetValue(filesystemPath, out var document))
        {
            return Task.FromResult(new CompletionList());
        }

        var context = _tokenAnalyzer.GetContext(document, request.Position);

        var completions = new List<CompletionItem>();

        if (context.TriggerChar == ".")
        {
            // Scenario: Alias.| -> Suggest types from the included document
            if (context.PreviousWord != null && document.IncludedDocuments.TryGetValue(context.PreviousWord, out var includedDoc))
            {
                completions.AddRange(GetTypesFromDocument(includedDoc));
            }
        }
        else if (context.IsTypePosition)
        {
            // Scenario: 1: | or list<| -> Suggest Types
            completions.AddRange(GetBaseTypes());
            completions.AddRange(GetTypesFromDocument(document));
            completions.AddRange(GetIncludes(document));
        }
        else if (context.IsTopLevel)
        {
            // Scenario: Top of file or between definitions -> Suggest Keywords
            completions.AddRange(GetKeywords());
        }
        else
        {
            // Fallback: inside a block but unsure of context? 
            // Often inside a service/struct block we want types for fields.
            // Let's suggest types as a safe default if we are inside braces.
            if (context.BraceBalance > 0)
            {
                completions.AddRange(GetBaseTypes());
                completions.AddRange(GetTypesFromDocument(document));
                completions.AddRange(GetIncludes(document));
            }
        }

        return Task.FromResult(new CompletionList(completions));
    }

    public override Task<CompletionItem> Handle(CompletionItem request, CancellationToken cancellationToken)
    {
        return Task.FromResult(request);
    }

    protected override CompletionRegistrationOptions CreateRegistrationOptions(CompletionCapability capability, ClientCapabilities clientCapabilities)
    {
        return new CompletionRegistrationOptions
        {
            DocumentSelector = new TextDocumentSelector(
                new TextDocumentFilter
                {
                    Pattern = "**/*.thrift"
                }
            ),
            ResolveProvider = false,
            TriggerCharacters = new Container<string>(".")
        };
    }

    private IEnumerable<CompletionItem> GetBaseTypes()
    {
        var types = new[] { "bool", "byte", "i8", "i16", "i32", "i64", "double", "string", "binary", "slist", "void", "list", "map", "set", "uuid" };
        return types.Select(t => new CompletionItem { Label = t, Kind = CompletionItemKind.Keyword, Detail = "Base Type" });
    }

    private IEnumerable<CompletionItem> GetKeywords()
    {
        var keywords = new[] { "namespace", "include", "cpp_include", "const", "typedef", "enum", "senum", "struct", "union", "exception", "service", "extends", "required", "optional", "oneway", "void", "throws" };
        return keywords.Select(k => new CompletionItem { Label = k, Kind = CompletionItemKind.Keyword });
    }

    private IEnumerable<CompletionItem> GetTypesFromDocument(Document doc)
    {
        var items = new List<CompletionItem>();

        foreach (var kvp in doc.Structs)
            items.Add(new CompletionItem { Label = kvp.Key, Kind = CompletionItemKind.Struct, Detail = kvp.Value.Description ?? "Struct" });

        foreach (var kvp in doc.Enums)
            items.Add(new CompletionItem { Label = kvp.Key, Kind = CompletionItemKind.Enum, Detail = kvp.Value.Description ?? "Enum" });

        foreach (var kvp in doc.Unions)
            items.Add(new CompletionItem { Label = kvp.Key, Kind = CompletionItemKind.Struct, Detail = kvp.Value.Description ?? "Union" });

        foreach (var kvp in doc.Exceptions)
            items.Add(new CompletionItem { Label = kvp.Key, Kind = CompletionItemKind.Struct, Detail = kvp.Value.Description ?? "Exception" });

        return items;
    }

    private IEnumerable<CompletionItem> GetIncludes(Document doc)
    {
        return doc.IncludedDocuments.Keys.Select(k => new CompletionItem { Label = k, Kind = CompletionItemKind.Module, Detail = "Include Alias" });
    }
}
