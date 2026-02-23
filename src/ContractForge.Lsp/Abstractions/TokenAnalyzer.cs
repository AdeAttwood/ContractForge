using Antlr4.Runtime;

using NetRpc.Core.Types;

using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace NetRpc.Lsp.Abstractions;

public interface ITokenAnalyzer
{
    Context GetContext(Document document, Position position);
    IToken? GetTokenAtPosition(Document document, Position position);
}

public class TokenAnalyzer : ITokenAnalyzer
{
    public Context GetContext(Document document, Position position)
    {
        var tokens = document.Tokens;
        var tokenIndex = GetTokenIndexBeforePosition(tokens, position);

        var context = new Context();

        // Calculate brace balance up to cursor to know if we are TopLevel
        int braceBalance = 0;
        for (int i = 0; i <= tokenIndex; i++)
        {
            if (i < 0 || i >= tokens.Count) continue;
            var t = tokens[i];
            if (t.Text == "{") braceBalance++;
            if (t.Text == "}") braceBalance--;
        }
        context.BraceBalance = braceBalance;
        if (braceBalance == 0) context.IsTopLevel = true;

        if (tokenIndex >= 0 && tokenIndex < tokens.Count)
        {
            var currentToken = tokens[tokenIndex];

            // Check for "."
            if (currentToken.Text == ".")
            {
                context.TriggerChar = ".";
                context.IsTopLevel = false;
                if (tokenIndex > 0) context.PreviousWord = tokens[tokenIndex - 1].Text;
                return context;
            }

            // Check for Type Indicators
            var text = currentToken.Text;
            if (text == ":" || text == "," || text == "<" || text == "(" ||
                text == "returns" || text == "throws" || text == "const" || text == "typedef")
            {
                context.IsTypePosition = true;
                context.IsTopLevel = false;
            }
        }

        return context;
    }

    public IToken? GetTokenAtPosition(Document document, Position position)
    {
        var tokens = document.Tokens;

        foreach (var t in tokens)
        {
            // EOF check might be needed if token type is exposed, checking text/channel for now
            if (string.IsNullOrEmpty(t.Text) || t.Text == "<EOF>") continue;

            int startLine = t.Line - 1; // 1-based to 0-based
            int startChar = t.Column;
            int endLine = startLine;
            int endChar = startChar + t.Text.Length;

            if (position.Line == startLine)
            {
                if (position.Character >= startChar && position.Character <= endChar)
                {
                    return t;
                }
            }
        }
        return null;
    }

    private int GetTokenIndexBeforePosition(List<IToken> tokens, Position position)
    {
        var tokenIndex = -1;

        for (int i = 0; i < tokens.Count; i++)
        {
            var t = tokens[i];
            // Skip EOF
            if (t.Type == -1) break;

            // t.Line is 1-based. Position.Line is 0-based.
            if (t.Line > position.Line + 1 || (t.Line == position.Line + 1 && t.Column >= position.Character))
            {
                tokenIndex = i - 1;
                break;
            }
        }

        if (tokenIndex == -1) tokenIndex = tokens.Count - 2; // Last real token before EOF
        return tokenIndex;
    }
}

public class Context
{
    public bool IsTopLevel { get; set; }
    public bool IsTypePosition { get; set; }
    public string? TriggerChar { get; set; }
    public string? PreviousWord { get; set; }
    public int BraceBalance { get; set; }
}