using System.Text;

using ContractForge.Core.CSharp;
using ContractForge.Core.Types;

using Inflector;

namespace ContractForge.Core.CSharp.Generators;

/// <summary>
/// Generates C# discriminated union types from union definitions
/// </summary>
public class CSharpUnionGenerator : ICSharpGenerator
{
    private readonly CSharpTypeMapper _typeMapper;
    private readonly CSharpCodeGenOptions _options;

    public CSharpUnionGenerator(CSharpTypeMapper typeMapper, CSharpCodeGenOptions options)
    {
        _typeMapper = typeMapper;
        _options = options;
    }

    public void Generate(StringBuilder builder, Document document, List<Error> errors, int indent)
    {
        var prefix = new string(' ', indent);

        foreach (var union in document.Unions.Values)
        {
            // Generate the discriminator enum
            builder.AppendFormat("{0}[JsonConverter(typeof(JsonStringEnumConverter))]\n", prefix);
            builder.AppendFormat("{0}public enum {1}Type\n", prefix, union.Identifier);
            builder.AppendFormat("{0}{{\n", prefix);

            foreach (var field in union.Fields.Values)
            {
                builder.AppendFormat("{0}    [JsonStringEnumMemberName(\"{1}\")]\n", prefix, field.Identifier);
                builder.AppendFormat("{0}    {1},\n", prefix, field.Identifier.Pascalize());
            }

            builder.AppendFormat("{0}}}\n", prefix);

            // Generate the union class
            CSharpDocumentationHelper.AppendXmlDocComment(builder, union.Description, indent);
            var classModifier = _options.UsePartialClasses ? "partial " : "";
            builder.AppendFormat("{0}[JsonConverter(typeof({1}JsonConverter))]\n", prefix, union.Identifier);
            builder.AppendFormat("{0}public {1}class {2}\n", prefix, classModifier, union.Identifier);
            builder.AppendFormat("{0}{{\n", prefix);

            builder.AppendFormat("{0}    [JsonPropertyName(\"type\")]\n", prefix);
            builder.AppendFormat("{0}    public {1}Type Type {{ get; set; }}\n\n", prefix, union.Identifier);

            foreach (var field in union.Fields.Values)
            {
                CSharpDocumentationHelper.AppendXmlDocComment(builder, field.Description, indent + 4);
                builder.AppendFormat("{0}    [JsonPropertyName(\"{1}\")]\n", prefix, field.Identifier);
                builder.AppendFormat("{0}    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]\n", prefix);
                builder.AppendFormat("{0}    public {1}? {2} {{ get; set; }}\n", prefix, _typeMapper.ToCSharpType(field.Type), field.Identifier.Pascalize());
                builder.AppendFormat("{0}    public {1}({2} {3})\n", prefix, union.Identifier, _typeMapper.ToCSharpType(field.Type), field.Identifier);
                builder.AppendFormat("{0}    {{\n", prefix);
                builder.AppendFormat("{0}        Type = {1}Type.{2};\n", prefix, union.Identifier, field.Identifier.Pascalize());
                builder.AppendFormat("{0}        this.{1} = {2};\n", prefix, field.Identifier.Pascalize(), field.Identifier);
                builder.AppendFormat("{0}    }}\n", prefix);
            }

            builder.AppendFormat("{0}}}\n", prefix);

            GenerateConverter(builder, union, prefix);
        }
    }

    private void GenerateConverter(StringBuilder builder, Union union, string prefix)
    {
        builder.AppendFormat("{0}public sealed class {1}JsonConverter : JsonConverter<{1}>\n", prefix, union.Identifier);
        builder.AppendFormat("{0}{{\n", prefix);
        builder.AppendFormat("{0}    public override {1} Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)\n", prefix, union.Identifier);
        builder.AppendFormat("{0}    {{\n", prefix);
        builder.AppendFormat("{0}        using var document = JsonDocument.ParseValue(ref reader);\n", prefix);
        builder.AppendFormat("{0}        if (document.RootElement.ValueKind != JsonValueKind.Object)\n", prefix);
        builder.AppendFormat("{0}        {{\n", prefix);
        builder.AppendFormat("{0}            throw new JsonException(\"{1} must be a JSON object.\");\n", prefix, union.Identifier);
        builder.AppendFormat("{0}        }}\n\n", prefix);
        builder.AppendFormat("{0}        JsonElement? discriminator = null;\n", prefix);
        builder.AppendFormat("{0}        JsonElement? branchPayload = null;\n", prefix);
        builder.AppendFormat("{0}        string? branchName = null;\n", prefix);
        builder.AppendFormat("{0}        var discriminatorCount = 0;\n", prefix);
        builder.AppendFormat("{0}        var branchCount = 0;\n\n", prefix);
        builder.AppendFormat("{0}        foreach (var property in document.RootElement.EnumerateObject())\n", prefix);
        builder.AppendFormat("{0}        {{\n", prefix);
        builder.AppendFormat("{0}            if (PropertyNameEquals(property.Name, \"type\", options))\n", prefix);
        builder.AppendFormat("{0}            {{\n", prefix);
        builder.AppendFormat("{0}                discriminator = property.Value;\n", prefix);
        builder.AppendFormat("{0}                discriminatorCount++;\n", prefix);
        builder.AppendFormat("{0}                continue;\n", prefix);
        builder.AppendFormat("{0}            }}\n", prefix);

        foreach (var field in union.Fields.Values)
        {
            builder.AppendFormat("{0}            if (PropertyNameEquals(property.Name, \"{1}\", options))\n", prefix, field.Identifier);
            builder.AppendFormat("{0}            {{\n", prefix);
            builder.AppendFormat("{0}                branchName = \"{1}\";\n", prefix, field.Identifier);
            builder.AppendFormat("{0}                branchPayload = property.Value;\n", prefix);
            builder.AppendFormat("{0}                branchCount++;\n", prefix);
            builder.AppendFormat("{0}            }}\n", prefix);
        }

        builder.AppendFormat("{0}        }}\n\n", prefix);
        builder.AppendFormat("{0}        if (discriminatorCount == 0)\n", prefix);
        builder.AppendFormat("{0}        {{\n", prefix);
        builder.AppendFormat("{0}            throw new JsonException(\"{1} is missing required discriminator 'type'.\");\n", prefix, union.Identifier);
        builder.AppendFormat("{0}        }}\n", prefix);
        builder.AppendFormat("{0}        if (discriminatorCount > 1)\n", prefix);
        builder.AppendFormat("{0}        {{\n", prefix);
        builder.AppendFormat("{0}            throw new JsonException(\"{1} contains multiple 'type' discriminators.\");\n", prefix, union.Identifier);
        builder.AppendFormat("{0}        }}\n", prefix);
        builder.AppendFormat("{0}        if (discriminator!.Value.ValueKind != JsonValueKind.String)\n", prefix);
        builder.AppendFormat("{0}        {{\n", prefix);
        builder.AppendFormat("{0}            throw new JsonException(\"{1} discriminator 'type' must be a string.\");\n", prefix, union.Identifier);
        builder.AppendFormat("{0}        }}\n\n", prefix);
        builder.AppendFormat("{0}        var discriminatorName = discriminator.Value.GetString();\n", prefix);
        builder.AppendFormat("{0}        if (discriminatorName is not (", prefix);
        builder.Append(string.Join(" or ", union.Fields.Values.Select(field => $"\"{field.Identifier}\"")));
        builder.Append("))\n");
        builder.AppendFormat("{0}        {{\n", prefix);
        builder.AppendFormat("{0}            throw new JsonException($\"Unknown {1} discriminator '{{discriminatorName}}'.\");\n", prefix, union.Identifier);
        builder.AppendFormat("{0}        }}\n", prefix);
        builder.AppendFormat("{0}        if (branchCount == 0)\n", prefix);
        builder.AppendFormat("{0}        {{\n", prefix);
        builder.AppendFormat("{0}            throw new JsonException($\"{1} branch '{{discriminatorName}}' is missing its payload.\");\n", prefix, union.Identifier);
        builder.AppendFormat("{0}        }}\n", prefix);
        builder.AppendFormat("{0}        if (branchCount > 1)\n", prefix);
        builder.AppendFormat("{0}        {{\n", prefix);
        builder.AppendFormat("{0}            throw new JsonException(\"{1} contains multiple branch payloads.\");\n", prefix, union.Identifier);
        builder.AppendFormat("{0}        }}\n", prefix);
        builder.AppendFormat("{0}        if (branchPayload!.Value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)\n", prefix);
        builder.AppendFormat("{0}        {{\n", prefix);
        builder.AppendFormat("{0}            throw new JsonException($\"{1} branch '{{discriminatorName}}' payload cannot be null.\");\n", prefix, union.Identifier);
        builder.AppendFormat("{0}        }}\n", prefix);
        builder.AppendFormat("{0}        if (!string.Equals(discriminatorName, branchName, StringComparison.Ordinal))\n", prefix);
        builder.AppendFormat("{0}        {{\n", prefix);
        builder.AppendFormat("{0}            throw new JsonException($\"{1} discriminator '{{discriminatorName}}' does not match branch '{{branchName}}'.\");\n", prefix, union.Identifier);
        builder.AppendFormat("{0}        }}\n\n", prefix);
        builder.AppendFormat("{0}        return discriminatorName switch\n", prefix);
        builder.AppendFormat("{0}        {{\n", prefix);
        foreach (var field in union.Fields.Values)
        {
            builder.AppendFormat("{0}            \"{1}\" => new {2}(DeserializePayload<{3}>(branchPayload.Value, options, \"{1}\")),\n", prefix, field.Identifier, union.Identifier, _typeMapper.ToCSharpType(field.Type));
        }
        builder.AppendFormat("{0}            _ => throw new JsonException($\"Unknown {1} discriminator '{{discriminatorName}}'.\"),\n", prefix, union.Identifier);
        builder.AppendFormat("{0}        }};\n", prefix);
        builder.AppendFormat("{0}    }}\n\n", prefix);

        builder.AppendFormat("{0}    public override void Write(Utf8JsonWriter writer, {1} value, JsonSerializerOptions options)\n", prefix, union.Identifier);
        builder.AppendFormat("{0}    {{\n", prefix);
        builder.AppendFormat("{0}        writer.WriteStartObject();\n", prefix);
        builder.AppendFormat("{0}        switch (value.Type)\n", prefix);
        builder.AppendFormat("{0}        {{\n", prefix);
        foreach (var field in union.Fields.Values)
        {
            var propertyName = field.Identifier.Pascalize();
            builder.AppendFormat("{0}            case {1}Type.{2}:\n", prefix, union.Identifier, propertyName);
            builder.AppendFormat("{0}                if (value.{1} is null)\n", prefix, propertyName);
            builder.AppendFormat("{0}                {{\n", prefix);
            builder.AppendFormat("{0}                    throw new JsonException(\"{1} branch '{2}' payload cannot be null.\");\n", prefix, union.Identifier, field.Identifier);
            builder.AppendFormat("{0}                }}\n", prefix);
            builder.AppendFormat("{0}                writer.WriteString(\"type\", \"{1}\");\n", prefix, field.Identifier);
            builder.AppendFormat("{0}                writer.WritePropertyName(\"{1}\");\n", prefix, field.Identifier);
            builder.AppendFormat("{0}                JsonSerializer.Serialize(writer, value.{1}, options);\n", prefix, propertyName);
            builder.AppendFormat("{0}                break;\n", prefix);
        }
        builder.AppendFormat("{0}            default:\n", prefix);
        builder.AppendFormat("{0}                throw new JsonException($\"Unknown {1} discriminator '{{value.Type}}'.\");\n", prefix, union.Identifier);
        builder.AppendFormat("{0}        }}\n", prefix);
        builder.AppendFormat("{0}        writer.WriteEndObject();\n", prefix);
        builder.AppendFormat("{0}    }}\n\n", prefix);

        builder.AppendFormat("{0}    private static T DeserializePayload<T>(JsonElement payload, JsonSerializerOptions options, string branchName)\n", prefix);
        builder.AppendFormat("{0}    {{\n", prefix);
        builder.AppendFormat("{0}        var value = payload.Deserialize<T>(options);\n", prefix);
        builder.AppendFormat("{0}        return value is null\n", prefix);
        builder.AppendFormat("{0}            ? throw new JsonException($\"{1} branch '{{branchName}}' payload cannot be null.\")\n", prefix, union.Identifier);
        builder.AppendFormat("{0}            : value;\n", prefix);
        builder.AppendFormat("{0}    }}\n\n", prefix);

        builder.AppendFormat("{0}    private static bool PropertyNameEquals(string actual, string expected, JsonSerializerOptions options)\n", prefix);
        builder.AppendFormat("{0}    {{\n", prefix);
        builder.AppendFormat("{0}        return string.Equals(actual, expected, options.PropertyNameCaseInsensitive\n", prefix);
        builder.AppendFormat("{0}            ? StringComparison.OrdinalIgnoreCase\n", prefix);
        builder.AppendFormat("{0}            : StringComparison.Ordinal);\n", prefix);
        builder.AppendFormat("{0}    }}\n", prefix);
        builder.AppendFormat("{0}}}\n", prefix);
    }
}