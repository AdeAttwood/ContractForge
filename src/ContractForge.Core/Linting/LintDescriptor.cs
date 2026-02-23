using NetRpc.Core.Types;

namespace NetRpc.Core.Linting;

public class LintDescriptor
{
    public string Id { get; }
    public Severity DefaultSeverity { get; }

    public LintDescriptor(string id, Severity defaultSeverity)
    {
        Id = id;
        DefaultSeverity = defaultSeverity;
    }
}