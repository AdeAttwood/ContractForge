using Antlr4.Runtime;

using NetRpc.Core.Linting;
using NetRpc.Core.Types;
using NetRpc.ThriftParser;

namespace NetRpc.Core.Thrift;

public class ThriftLoader : ILoader
{
    private readonly ThriftLinter _linter;

    public ThriftLoader()
    {
        _linter = new ThriftLinter();
    }

    public Document Load(string uri, DefinitionState state)
    {
        var document = new Document
        {
            Uri = uri,
            Content = File.ReadAllText(uri),
        };

        state.Documents[uri] = document;

        return Load(document, state);
    }

    public Document Load(Document document, DefinitionState state)
    {
        var lexer = new ThriftLexer(new AntlrInputStream(document.Content));
        lexer.RemoveErrorListeners();

        var tokens = new CommonTokenStream(lexer);
        var parser = new NetRpc.ThriftParser.ThriftParser(tokens);

        parser.RemoveErrorListeners();
        parser.AddParseListener(new ThriftListener(document, state));

        // Attach linter
        _linter.Attach(parser, document);

        parser.document();

        return document;
    }
}