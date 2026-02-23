using Antlr4.Runtime;

using NetRpc.Core.Types;
using NetRpc.Lsp.Abstractions;

using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace NetRpc.Lsp.Test;

public class TokenAnalyzerTests
{
    private readonly TokenAnalyzer _analyzer = new();

    [Fact]
    public void GetContext_AtStartOfFile_ReturnsTopLevel()
    {
        var doc = CreateDocument("");
        var context = _analyzer.GetContext(doc, new Position(0, 0));

        Assert.True(context.IsTopLevel);
        Assert.False(context.IsTypePosition);
    }

    [Fact]
    public void GetContext_AfterDot_ReturnsTriggerChar()
    {
        var doc = CreateDocument("Shared.");
        var context = _analyzer.GetContext(doc, new Position(0, 7));

        Assert.Equal(".", context.TriggerChar);
        Assert.Equal("Shared", context.PreviousWord);
    }

    [Fact]
    public void GetContext_InsideStruct_ReturnsTopLevelFalse()
    {
        var code = @"
struct User {
    1: 
}";
        var doc = CreateDocument(code);
        // Position at "1: |"
        var context = _analyzer.GetContext(doc, new Position(2, 7));

        Assert.False(context.IsTopLevel);
        Assert.Equal(1, context.BraceBalance);
    }

    [Fact]
    public void GetContext_AfterColon_ReturnsTypePosition()
    {
        var code = @"
struct User {
    1: 
}";
        var doc = CreateDocument(code);
        var context = _analyzer.GetContext(doc, new Position(2, 7));

        Assert.True(context.IsTypePosition);
    }

    [Fact]
    public void GetContext_AfterList_ReturnsTypePosition()
    {
        var code = @"
struct User {
    1: list<
}";
        var doc = CreateDocument(code);
        var context = _analyzer.GetContext(doc, new Position(2, 12));

        Assert.True(context.IsTypePosition);
    }

    [Fact]
    public void GetTokenAtPosition_OnIdentifier_ReturnsToken()
    {
        var code = "struct User {}";
        var doc = CreateDocument(code);
        // Position on "User"
        var token = _analyzer.GetTokenAtPosition(doc, new Position(0, 8));

        Assert.NotNull(token);
        Assert.Equal("User", token.Text);
    }

    private Document CreateDocument(string content)
    {
        var lexer = new NetRpc.ThriftParser.ThriftLexer(new AntlrInputStream(content));
        var tokens = new CommonTokenStream(lexer);
        tokens.Fill();

        return new Document
        {
            Uri = "test.thrift",
            Content = content,
            Tokens = tokens.GetTokens().ToList()
        };
    }
}