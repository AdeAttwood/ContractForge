using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

using ContractForge.Core.Types;

using Inflector;

using ThriftEnum = ContractForge.Core.Types.Enum;
using ThriftList = ContractForge.Core.Types.List;

namespace ContractForge.Core.OpenApi;

public class OpenApiCodeGen : ICodeGen
{
    private readonly OpenApiCodeGenOptions _options;

    public OpenApiCodeGen(OpenApiCodeGenOptions? options = null)
    {
        _options = options ?? new OpenApiCodeGenOptions();
    }

    public CodeGenResult Build(DefinitionState state)
    {
        Inflector.Inflector.SetDefaultCultureFunc = () => new CultureInfo("en-GB");

        var errors = ValidateComponentNames(state);
        var paths = BuildPaths(state, errors);

        if (errors.Count > 0)
        {
            return new CodeGenResult { Errors = errors };
        }

        var root = new JsonObject
        {
            ["openapi"] = _options.OpenApiVersion,
            ["info"] = new JsonObject
            {
                ["title"] = _options.Title,
                ["version"] = _options.ApiVersion
            },
            ["paths"] = paths,
            ["components"] = new JsonObject
            {
                ["schemas"] = BuildSchemas(state)
            }
        };

        var tags = BuildTags(state);
        if (tags.Count > 0)
        {
            root["tags"] = tags;
        }

        if (!string.IsNullOrWhiteSpace(_options.ServerUrl))
        {
            root["servers"] = new JsonArray
            {
                new JsonObject
                {
                    ["url"] = _options.ServerUrl
                }
            };
        }

        return new CodeGenResult
        {
            Output = root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine,
            Errors = errors
        };
    }

    private List<Error> ValidateComponentNames(DefinitionState state)
    {
        var errors = new List<Error>();
        var componentNames = new Dictionary<string, BaseType>(StringComparer.Ordinal);

        foreach (var document in state.Documents.Values)
        {
            foreach (var type in document.Structs.Values.Cast<BaseType>()
                .Concat(document.Exceptions.Values)
                .Concat(document.Enums.Values)
                .Concat(document.Unions.Values))
            {
                var name = GetComponentName(type);
                if (componentNames.ContainsKey(name))
                {
                    errors.Add(new Error(
                        type.Document,
                        type.Point,
                        $"OpenAPI schema component name '{name}' is defined more than once. Use unique Thrift type names."
                    ));
                }
                else
                {
                    componentNames.Add(name, type);
                }
            }
        }

        return errors;
    }

    private JsonArray BuildTags(DefinitionState state)
    {
        var tags = new JsonArray();
        var seenTags = new HashSet<string>(StringComparer.Ordinal);

        foreach (var document in state.Documents.Values)
        {
            foreach (var service in document.Services.Values)
            {
                if (!seenTags.Add(service.Identifier))
                {
                    continue;
                }

                var tag = new JsonObject
                {
                    ["name"] = service.Identifier
                };

                AddDescription(tag, service.Description);
                AddAuthorizePolicy(tag, service.Attributes.GetValueOrDefault("authorize.policy"));

                tags.Add(tag);
            }
        }

        return tags;
    }

    private JsonObject BuildSchemas(DefinitionState state)
    {
        var schemas = new JsonObject();

        foreach (var document in state.Documents.Values)
        {
            foreach (var structType in document.Structs.Values)
            {
                schemas[structType.Identifier] = BuildObjectSchema(structType);
            }

            foreach (var exceptionType in document.Exceptions.Values)
            {
                schemas[exceptionType.Identifier] = BuildObjectSchema(exceptionType);
            }

            foreach (var enumType in document.Enums.Values)
            {
                schemas[enumType.Identifier] = BuildEnumSchema(enumType);
            }

            foreach (var union in document.Unions.Values)
            {
                schemas[union.Identifier] = BuildUnionSchema(union);
            }
        }

        return schemas;
    }

    private JsonObject BuildObjectSchema(Struct structType)
    {
        var schema = new JsonObject
        {
            ["type"] = "object"
        };

        AddDescription(schema, structType.Description);

        var properties = new JsonObject();
        var required = new JsonArray();

        foreach (var field in structType.Fields.Values)
        {
            var propertySchema = ToSchema(field.Type);
            if (!field.IsRequired)
            {
                propertySchema = MakeNullable(propertySchema);
            }

            AddDescription(propertySchema, field.Description);
            properties[field.Identifier] = propertySchema;

            if (field.IsRequired)
            {
                required.Add(field.Identifier);
            }
        }

        schema["properties"] = properties;
        if (required.Count > 0)
        {
            schema["required"] = required;
        }

        return schema;
    }

    private JsonObject BuildEnumSchema(ThriftEnum enumType)
    {
        var schema = new JsonObject
        {
            ["type"] = "string",
            ["enum"] = CreateStringArray(enumType.Values.Values.Select(ToSerializedEnumValue))
        };

        AddDescription(schema, enumType.Description);

        return schema;
    }

    private JsonObject BuildUnionSchema(Union union)
    {
        var properties = new JsonObject
        {
            ["type"] = new JsonObject
            {
                ["type"] = "string",
                ["enum"] = CreateStringArray(union.Fields.Values.Select(field => field.Identifier))
            }
        };

        foreach (var field in union.Fields.Values)
        {
            var variantSchema = MakeNullable(ToSchema(field.Type));
            AddDescription(variantSchema, field.Description);
            properties[field.Identifier] = variantSchema;
        }

        var schema = new JsonObject
        {
            ["type"] = "object",
            ["required"] = new JsonArray { "type" },
            ["properties"] = properties
        };

        AddDescription(schema, union.Description);

        return schema;
    }

    private JsonObject BuildPaths(DefinitionState state, List<Error> errors)
    {
        var paths = new JsonObject();
        var operationIds = new HashSet<string>(StringComparer.Ordinal);

        foreach (var document in state.Documents.Values)
        {
            foreach (var service in document.Services.Values)
            {
                foreach (var func in service.Functions.Values)
                {
                    var method = func.Method().ToUpperInvariant();
                    if (method is not ("GET" or "POST"))
                    {
                        errors.Add(new Error(
                            document,
                            func.Point,
                            $"OpenAPI generator only supports GET and POST methods. Method '{method}' is not supported."
                        ));
                        continue;
                    }

                    if (method == "POST" && func.Parameters.Count > 1)
                    {
                        errors.Add(new Error(
                            document,
                            func.Point,
                            $"POST method '{func.Identifier}' has {func.Parameters.Count} parameters. POST methods can only have one request body parameter. Wrap multiple parameters in a struct instead."
                        ));
                        continue;
                    }

                    var operationId = $"{service.Identifier}_{func.Identifier}";
                    if (!operationIds.Add(operationId))
                    {
                        errors.Add(new Error(
                            document,
                            func.Point,
                            $"OpenAPI operationId '{operationId}' is defined more than once. Use unique service/function name combinations."
                        ));
                        continue;
                    }

                    var path = CombinePath(service.Url(), func.Url());
                    if (paths[path] is not JsonObject pathItem)
                    {
                        pathItem = new JsonObject();
                        paths[path] = pathItem;
                    }

                    var methodKey = method.ToLowerInvariant();
                    if (pathItem.ContainsKey(methodKey))
                    {
                        errors.Add(new Error(
                            document,
                            func.Point,
                            $"OpenAPI path '{path}' already defines a {method} operation."
                        ));
                        continue;
                    }

                    pathItem[methodKey] = BuildOperation(service, func, method, operationId);
                }
            }
        }

        return paths;
    }

    private JsonObject BuildOperation(Service service, Function func, string method, string operationId)
    {
        var operation = new JsonObject
        {
            ["operationId"] = operationId,
            ["tags"] = new JsonArray { service.Identifier },
            ["responses"] = BuildResponses(func)
        };

        AddDescription(operation, func.Description);
        AddAuthorizePolicy(operation, GetEffectiveAuthorizePolicy(service, func));

        if (method == "GET")
        {
            var parameters = BuildQueryParameters(func);
            if (parameters.Count > 0)
            {
                operation["parameters"] = parameters;
            }
        }
        else if (func.Parameters.Count == 1)
        {
            operation["requestBody"] = BuildRequestBody(func.Parameters.Values.Single());
        }

        return operation;
    }

    private JsonArray BuildQueryParameters(Function func)
    {
        var parameters = new JsonArray();

        foreach (var parameter in func.Parameters.Values)
        {
            AppendQueryParameters(parameters, parameter.Identifier, parameter.Type, true);
        }

        return parameters;
    }

    private void AppendQueryParameters(JsonArray parameters, string name, BaseType type, bool required)
    {
        if (type is Struct structType)
        {
            foreach (var field in structType.Fields.Values)
            {
                AppendQueryParameters(parameters, $"{name}.{field.Identifier}", field.Type, required && field.IsRequired);
            }

            return;
        }

        var parameter = new JsonObject
        {
            ["name"] = name,
            ["in"] = "query",
            ["required"] = required,
            ["schema"] = ToSchema(type)
        };

        if (type is ThriftList)
        {
            parameter["style"] = "form";
            parameter["explode"] = false;
        }

        parameters.Add(parameter);
    }

    private JsonObject BuildRequestBody(Field parameter)
    {
        return new JsonObject
        {
            ["required"] = true,
            ["content"] = new JsonObject
            {
                ["application/json"] = new JsonObject
                {
                    ["schema"] = ToSchema(parameter.Type)
                }
            }
        };
    }

    private JsonObject BuildResponses(Function func)
    {
        var responses = new JsonObject();

        if (IsVoid(func.Type))
        {
            responses["204"] = new JsonObject
            {
                ["description"] = "No Content"
            };
        }
        else
        {
            responses["200"] = new JsonObject
            {
                ["description"] = "OK",
                ["content"] = BuildSuccessContent(func.Type)
            };
        }

        if (func.Exceptions.Count > 0)
        {
            responses["default"] = new JsonObject
            {
                ["description"] = "Error",
                ["content"] = new JsonObject
                {
                    ["application/json"] = new JsonObject
                    {
                        ["schema"] = new JsonObject
                        {
                            ["oneOf"] = new JsonArray(func.Exceptions.Values.Select(exception => ToSchema(exception.Type)).ToArray<JsonNode?>())
                        }
                    }
                }
            };
        }

        return responses;
    }

    private JsonObject BuildSuccessContent(BaseType type)
    {
        var content = new JsonObject
        {
            ["application/json"] = new JsonObject
            {
                ["schema"] = ToSchema(type)
            }
        };

        if (type is ThriftList list)
        {
            content["application/x-ndjson"] = new JsonObject
            {
                ["schema"] = ToSchema(list.InnerType)
            };
        }

        return content;
    }

    private JsonObject ToSchema(BaseType type)
    {
        return type switch
        {
            Struct s => RefSchema(s.Identifier),
            Union u => RefSchema(u.Identifier),
            ThriftEnum e => RefSchema(e.Identifier),
            ThriftList l => new JsonObject
            {
                ["type"] = "array",
                ["items"] = ToSchema(l.InnerType)
            },
            Map m => new JsonObject
            {
                ["type"] = "object",
                ["additionalProperties"] = ToSchema(m.Value)
            },
            Primitive p => ToPrimitiveSchema(p),
            _ => throw new Exception($"Unable to convert {type} to an OpenAPI schema")
        };
    }

    private JsonObject ToPrimitiveSchema(Primitive primitive)
    {
        return primitive.Type switch
        {
            "i32" => new JsonObject
            {
                ["type"] = "integer",
                ["format"] = "int32"
            },
            "i64" => new JsonObject
            {
                ["type"] = "integer",
                ["format"] = "int64"
            },
            "double" => new JsonObject
            {
                ["type"] = "number",
                ["format"] = "double"
            },
            "string" => new JsonObject
            {
                ["type"] = "string"
            },
            "bool" => new JsonObject
            {
                ["type"] = "boolean"
            },
            "void" => new JsonObject(),
            _ => throw new Exception($"Invalid primitive value '{primitive.Type}' in OpenAPI codegen")
        };
    }

    private JsonObject MakeNullable(JsonObject schema)
    {
        if (_options.IsOpenApi31)
        {
            if (schema.TryGetPropertyValue("type", out var typeNode) && typeNode is JsonValue typeValue && typeValue.TryGetValue<string>(out var type))
            {
                schema["type"] = new JsonArray { type, "null" };
                return schema;
            }

            return new JsonObject
            {
                ["oneOf"] = new JsonArray
                {
                    schema,
                    new JsonObject { ["type"] = "null" }
                }
            };
        }

        if (schema.ContainsKey("$ref"))
        {
            return new JsonObject
            {
                ["allOf"] = new JsonArray { schema },
                ["nullable"] = true
            };
        }

        schema["nullable"] = true;
        return schema;
    }

    private static JsonObject RefSchema(string componentName)
    {
        return new JsonObject
        {
            ["$ref"] = $"#/components/schemas/{componentName}"
        };
    }

    private static string GetComponentName(BaseType type)
    {
        return type switch
        {
            Struct s => s.Identifier,
            Union u => u.Identifier,
            ThriftEnum e => e.Identifier,
            _ => throw new Exception($"Unable to get OpenAPI component name for {type}")
        };
    }

    private static JsonArray CreateStringArray(IEnumerable<string> values)
    {
        var array = new JsonArray();
        foreach (var value in values)
        {
            array.Add(value);
        }

        return array;
    }

    private static void AddDescription(JsonObject obj, string? description)
    {
        if (!string.IsNullOrWhiteSpace(description))
        {
            obj["description"] = description;
        }
    }

    private static void AddAuthorizePolicy(JsonObject obj, string? policy)
    {
        if (!string.IsNullOrWhiteSpace(policy))
        {
            obj["x-authorize-policy"] = policy;
        }
    }

    private static string? GetEffectiveAuthorizePolicy(Service service, Function func)
    {
        var policies = new[]
            {
                service.Attributes.GetValueOrDefault("authorize.policy"),
                func.Attributes.GetValueOrDefault("authorize.policy")
            }
            .Where(policy => !string.IsNullOrWhiteSpace(policy));

        return string.Join(", ", policies);
    }

    private static string ToSerializedEnumValue(EnumValue value)
    {
        return value.Name.Pascalize().Camelize();
    }

    private static bool IsVoid(BaseType type)
    {
        return type is Primitive primitive && primitive.Type == "void";
    }

    private static string CombinePath(string servicePath, string functionPath)
    {
        return $"/{servicePath.Trim('/')}/{functionPath.Trim('/')}";
    }
}