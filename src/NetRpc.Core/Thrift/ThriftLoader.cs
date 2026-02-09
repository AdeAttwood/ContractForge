using Antlr4.Runtime;

using NetRpc.Core.Types;
using NetRpc.ThriftParser;

namespace NetRpc.Core.Thrift;

public class ThriftLoader : ILoader
{
    public Document Load(string uri)
    {
        var document = new Document
        {
            Uri = uri,
            Content = File.ReadAllText(uri),
        };

        return Load(document);
    }

    public Document Load(Document document)
    {
        var lexer = new ThriftLexer(new AntlrInputStream(document.Content));
        lexer.RemoveErrorListeners();

        var tokens = new CommonTokenStream(lexer);
        var parser = new NetRpc.ThriftParser.ThriftParser(tokens);

        parser.RemoveErrorListeners();
        parser.AddErrorListener(new ThriftErrorListener(document));
        parser.AddParseListener(new ThriftListener(document));

        parser.document();

        return document;
    }
}