using NetRpc.Core.Thrift;
using NetRpc.Core.Types;
using NetRpc.ThriftParser;

namespace NetRpc.Core.Linting;

public abstract class LinterRule : ThriftBaseListener
{
    private Document? _document;
    public abstract LintDescriptor Descriptor { get; }
    protected Document Document => _document ?? throw new InvalidOperationException("LinterRule has not been initialized with a Document.");

    public void Initialize(Document document)
    {
        _document = document;
    }

    protected void ReportError(Point point, string message)
    {
        Document.Errors.Add(new Error(
            Document,
            point,
            message,
            Descriptor.Id,
            Descriptor.DefaultSeverity
        ));
    }
}