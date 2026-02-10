using NetRpc.Core.Linting.Rules;
using NetRpc.Core.Types;
using NetRpc.ThriftParser;

namespace NetRpc.Core.Linting;

public class ThriftLinter
{
    private readonly List<LinterRule> _rules = new();

    public ThriftLinter()
    {
        // Register default rules
        _rules.Add(new MissingCommaRule());
    }

    public void Attach(NetRpc.ThriftParser.ThriftParser parser, Document document)
    {
        // Attach standard error listener
        parser.RemoveErrorListeners();
        parser.AddErrorListener(new ThriftErrorListener(document));

        // Attach linter rules
        foreach (var rule in _rules)
        {
            rule.Initialize(document);
            parser.AddParseListener(rule);
        }
    }

    public void AddRule(LinterRule rule)
    {
        _rules.Add(rule);
    }
}