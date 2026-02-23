using ContractForge.Core.Linting.Rules;
using ContractForge.Core.Types;
using ContractForge.ThriftParser;

namespace ContractForge.Core.Linting;

public class ThriftLinter
{
    private readonly List<LinterRule> _rules = new();
    private readonly List<SemanticRule> _semanticRules = new();

    public ThriftLinter()
    {
        // Register default rules
        _rules.Add(new MissingCommaRule());
        _semanticRules.Add(new TypeResolutionRule());
    }

    public void Attach(ContractForge.ThriftParser.ThriftParser parser, Document document)
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

    public void RunSemanticAnalysis(Document document)
    {
        foreach (var rule in _semanticRules)
        {
            rule.Execute(document);
        }
    }

    public void AddRule(LinterRule rule)
    {
        _rules.Add(rule);
    }

    public void AddRule(SemanticRule rule)
    {
        _semanticRules.Add(rule);
    }
}