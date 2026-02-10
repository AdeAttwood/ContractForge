using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

using NetRpc.Core.Types;
using NetRpc.ThriftParser;

namespace NetRpc.Core.Thrift;

public class ThriftListener : ThriftBaseListener
{
    private readonly Document _document;
    private readonly DefinitionState _state;

    public ThriftListener(Document document, DefinitionState state)
    {
        _document = document;
        _state = state;
    }

    public override void ExitNamespace(ThriftParser.ThriftParser.NamespaceContext context)
    {
        var name = context.ID().GetText();
        if (_document.Namespaces.ContainsKey(name))
        {
            _document.Errors.Add(new Error(
                _document,
                ThriftListener.CreatePoint(context.ID().Symbol),
                $"Namespace for '{name}' as already been defined"
            ));
        }
        else
        {
            _document.Namespaces.Add(context.ID().GetText(), context.path().GetText());
        }
    }

    public override void ExitInclude(ThriftParser.ThriftParser.IncludeContext context)
    {
        var includePath = context.DOUBLE_QUOTE_STRING().GetText().Trim('"');
        var aliasToken = context.ID();

        var alias = aliasToken != null
            ? aliasToken.GetText()
            : Path.GetFileNameWithoutExtension(includePath);

        try
        {
            var absPath = _state.ResolveSourceFile(_document.Uri, includePath);
            var loadedDoc = _state.Load(absPath);
            _document.IncludedDocuments[alias] = loadedDoc;
        }
        catch (FileNotFoundException ex)
        {
            _document.Errors.Add(new Error(
               _document,
               ThriftListener.CreatePoint(context.DOUBLE_QUOTE_STRING().Symbol),
               $"Could not resolve include '{includePath}': {ex.Message}"
           ));
        }
    }

    public override void ExitStruct(ThriftParser.ThriftParser.StructContext context)
    {
        var structType = new Struct
        {
            Document = _document,
            Point = ThriftListener.CreatePoint(context.ID().Symbol),

            Identifier = context.ID().GetText(),
            Description = ExtractComment(context.DOC_COMMENT()?.GetText())
        };

        foreach (var field in context.fields()?.field() ?? [])
        {
            var fieldType = ThriftListener.BuildField(_document, field);
            structType.Fields.Add(fieldType.Identifier, fieldType);

        }

        foreach (var attribute in context.attributes()?.attribute() ?? [])
        {
            structType.Attributes.Add(
                attribute.path().GetText(),
                ThriftListener.EvaluateExpression(attribute.value())
            );
        }

        _document.Structs.Add(structType.Identifier, structType);
    }

    public override void ExitExceptionStruct(ThriftParser.ThriftParser.ExceptionStructContext context)
    {
        var exceptionType = new ExceptionStruct
        {
            Document = _document,
            Point = ThriftListener.CreatePoint(context.ID().Symbol),

            Identifier = context.ID().GetText(),
            Description = ExtractComment(context.DOC_COMMENT()?.GetText())
        };

        foreach (var field in context.fields()?.field() ?? [])
        {
            var fieldType = ThriftListener.BuildField(_document, field);
            exceptionType.Fields.Add(fieldType.Identifier, fieldType);

        }

        foreach (var attribute in context.attributes()?.attribute() ?? [])
        {
            exceptionType.Attributes.Add(
                attribute.path().GetText(),
                ThriftListener.EvaluateExpression(attribute.value())
            );
        }

        _document.Exceptions.Add(exceptionType.Identifier, exceptionType);
    }

    public override void ExitService(ThriftParser.ThriftParser.ServiceContext context)
    {
        var service = new Service
        {
            Document = _document,
            Point = ThriftListener.CreatePoint(context.ID().Symbol),

            Identifier = context.ID().GetText(),
            Description = ExtractComment(context.DOC_COMMENT()?.GetText())
        };

        var functions = context.functions();
        if (functions != null)
        {
            foreach (var func in functions.function())
            {
                var funcType = new Function
                {
                    Document = _document,
                    Point = ThriftListener.CreatePoint(func.ID().Symbol),

                    Identifier = func.ID().GetText(),
                    Description = ExtractComment(func.DOC_COMMENT()?.GetText()),
                    Type = ThriftListener.BuildType(_document, func.type()),
                };

                var funcFields = func.fields();
                if (funcFields != null)
                {
                    foreach (var field in funcFields.field())
                    {
                        var fieldType = ThriftListener.BuildField(_document, field);
                        funcType.Parameters.Add(fieldType.Identifier, fieldType);
                    }
                }

                var funcThrow = func.throwsFields();
                if (funcThrow != null && funcThrow.fields() != null)
                {
                    foreach (var field in funcThrow.fields().field())
                    {
                        var fieldType = ThriftListener.BuildField(_document, field);
                        funcType.Exceptions.Add(fieldType.Identifier, fieldType);
                    }
                }

                foreach (var attribute in func.attributes()?.attribute() ?? [])
                {
                    funcType.Attributes.Add(
                        attribute.path().GetText(),
                        ThriftListener.EvaluateExpression(attribute.value())
                    );
                }

                service.Functions.Add(funcType.Identifier, funcType);
            }
        }

        foreach (var attribute in context.attributes()?.attribute() ?? [])
        {
            service.Attributes.Add(
                attribute.path().GetText(),
                ThriftListener.EvaluateExpression(attribute.value())
            );
        }

        _document.Services.Add(service.Identifier, service);
    }

    public override void ExitUnion(ThriftParser.ThriftParser.UnionContext context)
    {
        var union = new Union
        {
            Document = _document,
            Point = ThriftListener.CreatePoint(context.ID().Symbol),

            Identifier = context.ID().GetText(),
            Description = ExtractComment(context.DOC_COMMENT()?.GetText())
        };

        foreach (var field in context.fields()?.field() ?? [])
        {
            var fieldType = ThriftListener.BuildField(_document, field);
            union.Fields.Add(fieldType.Identifier, fieldType);
        }

        _document.Unions.Add(union.Identifier, union);

    }

    public override void ExitEnum(ThriftParser.ThriftParser.EnumContext context)
    {
        var enumType = new Types.Enum
        {
            Document = _document,
            Point = ThriftListener.CreatePoint(context.ID().Symbol),
            Identifier = context.ID().GetText(),
            Description = ExtractComment(context.DOC_COMMENT()?.GetText())
        };

        // Parse enum values with auto-increment from 0
        var currentValue = 0;
        var enumValues = context.enumValues();
        if (enumValues != null)
        {
            foreach (var enumValue in enumValues.enumValue())
            {
                var name = enumValue.ID().GetText();
                var numberContext = enumValue.number();

                // Use explicit value or auto-increment
                if (numberContext != null)
                {
                    currentValue = int.Parse(numberContext.GetText());
                }

                var value = new EnumValue
                {
                    Name = name,
                    Value = currentValue,
                    Description = ExtractComment(enumValue.DOC_COMMENT()?.GetText())
                };

                enumType.Values.Add(name, value);
                currentValue++; // Auto-increment for next value
            }
        }

        // Parse attributes (for future extensibility)
        foreach (var attribute in context.attributes()?.attribute() ?? [])
        {
            enumType.Attributes.Add(
                attribute.path().GetText(),
                ThriftListener.EvaluateExpression(attribute.value())
            );
        }

        _document.Enums.Add(enumType.Identifier, enumType);
    }

    public static Field BuildField(Document document, ThriftParser.ThriftParser.FieldContext field)
    {
        var fieldType = new Field
        {
            Document = document,
            Point = ThriftListener.CreatePoint(field.ID().Symbol),

            Index = 0,
            Identifier = field.ID().GetText(),
            Description = ExtractComment(field.DOC_COMMENT()?.GetText()),
            IsRequired = field.visibility()?.GetText() == "required",
            Type = ThriftListener.BuildType(document, field.type()),
        };

        foreach (var attribute in field.attributes()?.attribute() ?? [])
        {
            fieldType.Attributes.Add(
                attribute.path().GetText(),
                ThriftListener.EvaluateExpression(attribute.value())
            );
        }

        return fieldType;
    }

    public static BaseType BuildType(Document document, ThriftParser.ThriftParser.TypeContext context)
    {
        var baseType = context.BASE_TYPE();
        if (baseType is not null)
        {
            return new Primitive
            {
                Document = document,
                Point = ThriftListener.CreatePoint(baseType.Symbol),

                Type = baseType.GetText(),
            };
        }

        var voidType = context.VOID_CONSTANT();
        if (voidType is not null)
        {
            return new Primitive
            {
                Document = document,
                Point = ThriftListener.CreatePoint(voidType.Symbol),

                Type = "void",
            };
        }

        var identifier = context.ID();
        if (identifier is not null)
        {
            var type = document.Resolve(identifier.GetText());
            if (type is not null)
            {
                return type;
            }

            document.Errors.Add(new Error(
                document,
                ThriftListener.CreatePoint(context),
                $"Type '{identifier.GetText()}' does not exist in the current context"
            ));

            return new Unknown
            {
                Document = document,
                Point = ThriftListener.CreatePoint(identifier.Symbol),
            };
        }

        var path = context.path();
        if (path is not null)
        {
            var typeName = path.GetText();
            var type = document.Resolve(typeName);
            if (type is not null)
            {
                return type;
            }

            document.Errors.Add(new Error(
                document,
                ThriftListener.CreatePoint(path.ID().First().Symbol),
                $"Type '{typeName}' does not exist in the current context (or included documents)"
            ));

            return new Unknown
            {
                Document = document,
                Point = ThriftListener.CreatePoint(path.ID().First().Symbol),
            };
        }

        var list = context.list();
        if (list is not null)
        {
            return new List
            {
                Document = document,
                Point = ThriftListener.CreatePoint(list),

                InnerType = ThriftListener.BuildType(document, list.type())
            };
        }

        var map = context.map();
        if (map is not null)
        {
            var types = map.type();
            return new Map
            {
                Document = document,
                Point = ThriftListener.CreatePoint(map),

                Key = ThriftListener.BuildType(document, types[0]),
                Value = ThriftListener.BuildType(document, types[1])
            };
        }

        throw new ArgumentException("Invalid context, we should never get here!! " + context.GetText());
    }

    public static string EvaluateExpression(ThriftParser.ThriftParser.ValueContext context)
    {
        var doubleQuotedString = context.DOUBLE_QUOTE_STRING();
        if (doubleQuotedString is not null)
        {
            return doubleQuotedString.GetText().Trim('"');
        }

        return context.GetText();
    }

    public static string? ExtractComment(string? comment)
    {
        if (comment == null) return null;

        // Remove opening /** and closing */
        var content = comment.Replace("\r\n", "\n").Trim();
        if (content.StartsWith("/**")) content = content.Substring(3);
        if (content.EndsWith("*/")) content = content.Substring(0, content.Length - 2);

        var lines = content.Split('\n').Select(line =>
        {
            var trimmed = line.TrimStart();
            // Remove leading "* " (star and space)
            if (trimmed.StartsWith("* ")) return trimmed.Substring(2).TrimEnd();
            // Handle lines with just "*" (empty comment lines)
            if (trimmed.StartsWith("*")) return trimmed.Substring(1).TrimEnd();

            return trimmed.TrimEnd();
        }).ToList();

        // Remove leading empty lines
        while (lines.Count > 0 && string.IsNullOrWhiteSpace(lines[0]))
        {
            lines.RemoveAt(0);
        }

        // Remove trailing empty lines
        while (lines.Count > 0 && string.IsNullOrWhiteSpace(lines[lines.Count - 1]))
        {
            lines.RemoveAt(lines.Count - 1);
        }

        return lines.Count == 0 ? null : string.Join('\n', lines);
    }

    public static Point CreatePoint(IToken token)
    {
        return new Point
        {
            Line = token.Line,
            Column = token.Column,
            Length = token.Text.Length
        };
    }

    public static Point CreatePoint(ParserRuleContext context)
    {
        var length = context.Start.Line == context.Stop.Line ? context.Stop.Column - context.Start.Column : 1;
        return new Point
        {
            Line = context.Start.Line,
            Column = context.Start.Column,
            Length = context.GetText().Length
        };
    }
}