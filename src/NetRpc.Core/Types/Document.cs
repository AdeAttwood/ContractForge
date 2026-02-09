namespace NetRpc.Core.Types;

public class Document
{
    public required string Uri { get; set; }
    public required string Content { get; set; }

    public List<Error> Errors { get; set; } = new();

    public Dictionary<string, string> Namespaces = new();
    public Dictionary<string, Struct> Structs = new();
    public Dictionary<string, ExceptionStruct> Exceptions = new();
    public Dictionary<string, Enum> Enums = new();
    public Dictionary<string, Union> Unions = new();
    public Dictionary<string, Service> Services = new();

    public BaseType? Resolve(string type)
    {
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