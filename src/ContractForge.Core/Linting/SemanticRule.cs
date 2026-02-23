using NetRpc.Core.Types;

namespace NetRpc.Core.Linting;

public abstract class SemanticRule
{
    public abstract LintDescriptor Descriptor { get; }

    public abstract void Execute(Document document);

    protected void ReportError(Document document, Point point, string message)
    {
        document.Errors.Add(new Error(
            document,
            point,
            message,
            Descriptor.Id,
            Descriptor.DefaultSeverity
        ));
    }
}