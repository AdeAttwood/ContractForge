using ContractForge.Core.Types;

namespace ContractForge.Core.Linting;

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