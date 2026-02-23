using NetRpc.Core.Types;

namespace NetRpc.Core;

public class CodeGenResult
{
    public string Output { get; set; } = String.Empty;
    public List<Error> Errors { get; set; } = new();
}