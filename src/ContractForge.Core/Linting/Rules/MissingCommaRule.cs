using Antlr4.Runtime.Tree;

using ContractForge.Core.Thrift;
using ContractForge.Core.Types;
using ContractForge.ThriftParser;

namespace ContractForge.Core.Linting.Rules;

public class MissingCommaRule : LinterRule
{
    public override LintDescriptor Descriptor => new LintDescriptor(
        "NR0001",
        Severity.Error
    );

    public override void ExitFields(ContractForge.ThriftParser.ThriftParser.FieldsContext context) => ValidateCommaSeparation(context);
    public override void ExitFunctions(ContractForge.ThriftParser.ThriftParser.FunctionsContext context) => ValidateCommaSeparation(context);
    public override void ExitEnumValues(ContractForge.ThriftParser.ThriftParser.EnumValuesContext context) => ValidateCommaSeparation(context);
    public override void ExitAttributes(ContractForge.ThriftParser.ThriftParser.AttributesContext context) => ValidateCommaSeparation(context);

    private void ValidateCommaSeparation(Antlr4.Runtime.ParserRuleContext context)
    {
        if (context.children == null) return;

        Antlr4.Runtime.ParserRuleContext? lastItem = null;
        foreach (var child in context.children)
        {
            if (child is ITerminalNode terminal && terminal.Symbol.Type == ContractForge.ThriftParser.ThriftParser.COMMA)
            {
                lastItem = null;
            }
            else if (child is Antlr4.Runtime.ParserRuleContext currentItem)
            {
                if (lastItem != null)
                {
                    ReportError(ThriftListener.CreatePoint(lastItem.Stop), "Missing comma");
                }
                lastItem = currentItem;
            }
        }
    }
}