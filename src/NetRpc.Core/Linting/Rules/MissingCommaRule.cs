using Antlr4.Runtime.Tree;

using NetRpc.Core.Thrift;
using NetRpc.Core.Types;
using NetRpc.ThriftParser;

namespace NetRpc.Core.Linting.Rules;

public class MissingCommaRule : LinterRule
{
    public override LintDescriptor Descriptor => new LintDescriptor(
        "NR0001",
        Severity.Error
    );

    public override void ExitFields(NetRpc.ThriftParser.ThriftParser.FieldsContext context) => ValidateCommaSeparation(context);
    public override void ExitFunctions(NetRpc.ThriftParser.ThriftParser.FunctionsContext context) => ValidateCommaSeparation(context);
    public override void ExitEnumValues(NetRpc.ThriftParser.ThriftParser.EnumValuesContext context) => ValidateCommaSeparation(context);
    public override void ExitAttributes(NetRpc.ThriftParser.ThriftParser.AttributesContext context) => ValidateCommaSeparation(context);

    private void ValidateCommaSeparation(Antlr4.Runtime.ParserRuleContext context)
    {
        if (context.children == null) return;

        Antlr4.Runtime.ParserRuleContext? lastItem = null;
        foreach (var child in context.children)
        {
            if (child is ITerminalNode terminal && terminal.Symbol.Type == NetRpc.ThriftParser.ThriftParser.COMMA)
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