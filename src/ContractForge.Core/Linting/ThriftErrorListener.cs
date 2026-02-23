using Antlr4.Runtime;

using NetRpc.Core.Thrift;
using NetRpc.Core.Types;

namespace NetRpc.Core.Linting;

public class ThriftErrorListener : BaseErrorListener
{
    private readonly Document _document;

    public ThriftErrorListener(Document document)
    {
        _document = document;
    }

    public override void SyntaxError(
        TextWriter output,
        IRecognizer recognizer,
        IToken offendingSymbol,
        int line,
        int column,
        string msg,
        RecognitionException e
    )
    {
        _document.Errors.Add(new Error(
            _document,
            ThriftListener.CreatePoint(offendingSymbol),
            $"Syntax error, {msg}",
            "NR0000",
            Severity.Error
        ));
    }
}