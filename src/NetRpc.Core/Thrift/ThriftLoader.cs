using Antlr4.Runtime;

using NetRpc.Core.Types;
using NetRpc.ThriftParser;

namespace NetRpc.Core.Thrift;

public class ThriftLoader : ILoader
{
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
        parser.AddErrorListener(new ThriftErrorListener(document));
        parser.AddParseListener(new ThriftListener(document, state));

        parser.document();

        return document;
    }
}