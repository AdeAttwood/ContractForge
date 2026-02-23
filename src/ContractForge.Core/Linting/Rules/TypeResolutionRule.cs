using ContractForge.Core.Types;

namespace ContractForge.Core.Linting.Rules;

public class TypeResolutionRule : SemanticRule
{
    public override LintDescriptor Descriptor => new LintDescriptor(
        "NR0002",
        Severity.Error
    );

    public override void Execute(Document document)
    {
        // Check Structs
        foreach (var structure in document.Structs.Values)
        {
            ValidateFields(structure.Fields, document);
        }

        // Check Exceptions
        foreach (var exception in document.Exceptions.Values)
        {
            ValidateFields(exception.Fields, document);
        }

        // Check Unions
        foreach (var union in document.Unions.Values)
        {
            ValidateFields(union.Fields, document);
        }

        // Check Services
        foreach (var service in document.Services.Values)
        {
            foreach (var func in service.Functions.Values)
            {
                func.Type = ResolveType(func.Type, document);

                ValidateFields(func.Parameters, document);
                ValidateFields(func.Exceptions, document);
            }
        }
    }

    private void ValidateFields(Dictionary<string, Field> fields, Document document)
    {
        foreach (var field in fields.Values)
        {
            field.Type = ResolveType(field.Type, document);
        }
    }

    private BaseType ResolveType(BaseType type, Document document)
    {
        if (type is List list)
        {
            list.InnerType = ResolveType(list.InnerType, document);

            return list;
        }
        else if (type is Map map)
        {
            map.Key = ResolveType(map.Key, document);
            map.Value = ResolveType(map.Value, document);

            return map;
        }
        else if (type is Unknown unknownType)
        {
            var resolved = document.Resolve(unknownType.Identifier);
            if (resolved != null)
            {
                return resolved;
            }
            else
            {
                ReportError(document, unknownType.Point, $"Type '{unknownType.Identifier}' does not exist in the current context");
                return unknownType;
            }
        }

        return type;
    }
}