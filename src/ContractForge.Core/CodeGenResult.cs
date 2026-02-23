using ContractForge.Core.Types;

namespace ContractForge.Core;

public class CodeGenResult
{
    public string Output { get; set; } = String.Empty;
    public List<Error> Errors { get; set; } = new();
}