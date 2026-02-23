using Antlr4.Runtime;

namespace ContractForge.Core.Types;

public class Document
{
    public required string Uri { get; set; }
    public required string Content { get; set; }

    public List<IToken> Tokens { get; set; } = new();

    public List<Error> Errors { get; set; } = new();

    public Dictionary<string, string> Namespaces = new();
    public Dictionary<string, Struct> Structs = new();
    public Dictionary<string, ExceptionStruct> Exceptions = new();
    public Dictionary<string, Enum> Enums = new();
    public Dictionary<string, Union> Unions = new();
    public Dictionary<string, Service> Services = new();
    public Dictionary<string, Document> IncludedDocuments = new();

    public BaseType? Resolve(string type)
    {
        if (type.Contains("."))
        {
            var parts = type.Split('.', 2);
            var alias = parts[0];
            var typeName = parts[1];

            if (IncludedDocuments.TryGetValue(alias, out var includedDoc))
            {
                return includedDoc.Resolve(typeName);
            }
        }

        if (Structs.TryGetValue(type, out var structType))
        {
            return structType;
        }

        if (Exceptions.TryGetValue(type, out var exceptionType))
        {
            return exceptionType;
        }

        if (Enums.TryGetValue(type, out var enumType))
        {
            return enumType;
        }

        if (Unions.TryGetValue(type, out var unionType))
        {
            return unionType;
        }

        return null;
    }
}