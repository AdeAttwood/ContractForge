using Antlr4.Runtime;

using NetRpc.Core;
using NetRpc.Core.Types;
using NetRpc.Lsp.Abstractions;

using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace NetRpc.Lsp;

public class ReferenceHandler : ReferencesHandlerBase
{
    private readonly DefinitionState _definitionState;
    private readonly ITokenAnalyzer _tokenAnalyzer;

    public ReferenceHandler(DefinitionState definitionState, ITokenAnalyzer tokenAnalyzer)
    {
        _definitionState = definitionState;
        _tokenAnalyzer = tokenAnalyzer;
    }

    public override Task<LocationContainer?> Handle(ReferenceParams request, CancellationToken cancellationToken)
    {
        var locations = new LocationContainer();

        var filesystemPath = DocumentUri.GetFileSystemPath(request.TextDocument);
        if (filesystemPath == null || !_definitionState.Documents.TryGetValue(filesystemPath, out var document))
        {
            return Task.FromResult<LocationContainer?>(locations);
        }

        // 1. Identify the Symbol at Cursor
        var token = _tokenAnalyzer.GetTokenAtPosition(document, request.Position);
        if (token == null || !IsIdentifier(token)) return Task.FromResult<LocationContainer?>(locations);

        string lookup = token.Text;

        // Handle "Alias.Type"
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

        object? targetSymbol = null;

        // Try Resolve Type
        targetSymbol = document.Resolve(lookup);

        // Try Resolve Service
        if (targetSymbol == null)
        {
            if (lookup.Contains("."))
            {
                var parts = lookup.Split('.', 2);
                if (document.IncludedDocuments.TryGetValue(parts[0], out var incDoc))
                {
                    incDoc.Services.TryGetValue(parts[1], out var s);
                    targetSymbol = s;
                }
            }
            else
            {
                document.Services.TryGetValue(lookup, out var s);
                targetSymbol = s;
            }
        }

        if (targetSymbol == null) return Task.FromResult<LocationContainer?>(locations);

        var resultList = new List<Location>();
        if (request.Context.IncludeDeclaration)
        {
            if (targetSymbol is BaseType bt) resultList.Add(CreateLocation(bt.Document.Uri, bt.Point));
        }

        // 2. Scan Workspace for Usages
        foreach (var doc in _definitionState.Documents.Values)
        {
            // Scan Structs/Unions/Exceptions (Fields)
            foreach (var structType in doc.Structs.Values) ScanFields(doc, structType.Fields.Values, targetSymbol, resultList);
            foreach (var unionType in doc.Unions.Values) ScanFields(doc, unionType.Fields.Values, targetSymbol, resultList);
            foreach (var exType in doc.Exceptions.Values) ScanFields(doc, exType.Fields.Values, targetSymbol, resultList);

            // Scan Services (Functions: Return Type, Params, Exceptions)
            foreach (var service in doc.Services.Values)
            {
                foreach (var func in service.Functions.Values)
                {
                    // Return Type
                    ScanType(doc, func.Type, func.Point, targetSymbol, resultList, true); // True = look before? Function point is name. Type is before.

                    // Parameters
                    ScanFields(doc, func.Parameters.Values, targetSymbol, resultList);

                    // Exceptions
                    ScanFields(doc, func.Exceptions.Values, targetSymbol, resultList);
                }
            }
        }

        return Task.FromResult<LocationContainer?>(LocationContainer.From(resultList));
    }

    private void ScanFields(Document doc, IEnumerable<Field> fields, object target, List<Location> locations)
    {
        foreach (var field in fields)
        {
            // Field Point is the ID. The type is BEFORE the ID.
            ScanType(doc, field.Type, field.Point, target, locations, true);
        }
    }

    private void ScanType(Document doc, BaseType? type, Point usageContextPoint, object target, List<Location> locations, bool lookBefore)
    {
        if (type == null) return;

        // Recursion for Containers (List/Map)
        if (type is NetRpc.Core.Types.List list)
        {
            // For list<Inner>, the usageContextPoint is likely the field name. 
            // We need to find "list" and then inner type.
            // This location finding is tricky without AST node location.
            // We'll skip complex container location accuracy for now and just check direct usage.
            ScanType(doc, list.InnerType, usageContextPoint, target, locations, lookBefore);
            return;
        }
        else if (type is NetRpc.Core.Types.Map map)
        {
            ScanType(doc, map.Key, usageContextPoint, target, locations, lookBefore);
            ScanType(doc, map.Value, usageContextPoint, target, locations, lookBefore);
            return;
        }

        // Direct Match check
        if (ReferenceEquals(type, target))
        {
            // We found a usage! Now we need to find WHERE in `doc` it is.
            // usageContextPoint is the anchor (e.g. Field Name). 
            // The type text should be `((BaseType)target).Identifier`.

            if (target is BaseType bt)
            {
                var targetName = "";
                if (bt is Struct s) targetName = s.Identifier;
                if (bt is NetRpc.Core.Types.Enum e) targetName = e.Identifier;
                // etc..

                // Find token `targetName` near `usageContextPoint`
                var loc = FindTokenNear(doc, targetName, usageContextPoint, lookBefore);
                if (loc != null) locations.Add(loc);
            }
        }
    }

    private Location? FindTokenNear(Document doc, string text, Point anchor, bool lookBefore)
    {
        var tokens = doc.Tokens;
        // Find anchor token index
        var anchorIndex = -1;
        // This linear scan is slow, optimizing later
        for (int i = 0; i < tokens.Count; i++)
        {
            if (tokens[i].Line == anchor.Line && tokens[i].Column == anchor.Column)
            {
                anchorIndex = i;
                break;
            }
        }

        if (anchorIndex == -1) return null;

        if (lookBefore)
        {
            // Scan backwards from anchor
            for (int i = anchorIndex - 1; i >= 0; i--)
            {
                if (tokens[i].Text == text)
                {
                    return CreateLocation(doc.Uri, new Point { Line = tokens[i].Line, Column = tokens[i].Column, Length = tokens[i].Text.Length });
                }
                // Stop if we hit structure boundaries to avoid false positives?
                // Or just a limit count
                if (anchorIndex - i > 20) break;
            }
        }

        return null;
    }

    protected override ReferenceRegistrationOptions CreateRegistrationOptions(ReferenceCapability capability, ClientCapabilities clientCapabilities)
    {
        return new ReferenceRegistrationOptions
        {
            DocumentSelector = new TextDocumentSelector(
                new TextDocumentFilter
                {
                    Pattern = "**/*.thrift"
                }
            )
        };
    }

    private Location CreateLocation(string uri, Point point)
    {
        var startLine = point.Line - 1;
        var startCol = point.Column;
        var endLine = startLine;
        var endCol = startCol + point.Length;

        return new Location
        {
            Uri = DocumentUri.From(uri),
            Range = new Range(startLine, startCol, endLine, endCol)
        };
    }

    private bool IsIdentifier(IToken token)
    {
        var text = token.Text;
        if (string.IsNullOrEmpty(text)) return false;
        char first = text[0];
        return char.IsLetter(first) || first == '_';
    }
}