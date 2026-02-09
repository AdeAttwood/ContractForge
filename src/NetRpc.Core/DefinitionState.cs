using NetRpc.Core.Thrift;
using NetRpc.Core.Types;

namespace NetRpc.Core;

public class DefinitionState
{
    public Dictionary<string, Document> Documents { get; set; } = new();

    public void Load(string uri)
    {
        this.Documents.Add(uri, GetLoader(uri).Load(uri));
    }

    private ILoader GetLoader(string uri)
    {
        return Path.GetExtension(uri) switch
        {
            ".thrift" => new ThriftLoader(),
            _ => throw new ArgumentException($"Unable to load '{uri}' no loader found"),
        };
    }
}