using ContractForge.Core.Types;

namespace ContractForge.Core;

public interface ILoader
{
    public Document Load(string uri, DefinitionState state);
}