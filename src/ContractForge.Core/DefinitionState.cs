using ContractForge.Core.Thrift;
using ContractForge.Core.Types;

namespace ContractForge.Core;

public class DefinitionState
{
    public Dictionary<string, Document> Documents { get; set; } = new();

    public List<string> IncludePaths { get; private set; } = new();

    public void AddIncludePath(string path)
    {
        IncludePaths.Add(Path.GetFullPath(path));
    }

    public Document Load(string uri)
    {
        var absUri = Path.GetFullPath(uri);
        if (Documents.TryGetValue(absUri, out var document))
        {
            return document;
        }

        return GetLoader(absUri).Load(absUri, this);
    }

    public string ResolveSourceFile(string referrerUri, string includePath)
    {
        // 1. Check relative to referrer
        var dir = Path.GetDirectoryName(referrerUri);
        if (dir != null)
        {
            var path = Path.Combine(dir, includePath);
            if (File.Exists(path)) return Path.GetFullPath(path);
        }

        // 2. Check include paths
        foreach (var inc in IncludePaths)
        {
            var path = Path.Combine(inc, includePath);
            if (File.Exists(path)) return Path.GetFullPath(path);
        }

        throw new FileNotFoundException($"Could not find included file '{includePath}'", includePath);
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