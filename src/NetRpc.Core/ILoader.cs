using NetRpc.Core.Types;

namespace NetRpc.Core;

public interface ILoader
{
    public Document Load(string uri);
}