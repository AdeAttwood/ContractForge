using System.Text.Json.Nodes;

using ContractForge.Core.OpenApi;
using ContractForge.Core.Test.TestHelpers;

namespace ContractForge.Core.Test.OpenApi;

public class OpenApiCodeGenTest : CodeGenTestBase
{
    [Fact]
    public void Generate_BasicService_ProducesOpenApiDocument()
    {
        var result = GenerateOpenApi(@"
            struct User {
                1: required i64 id,
                2: optional string name
            }

            struct SearchRequest {
                1: required string query
            }

            exception ValidationError {
                1: required string message
            }

            service UserService {
                User getUser(i64 id) (http.method = ""get""),
                list<User> searchUsers(SearchRequest request) throws (1: ValidationError error),
                void deleteUser(i64 id) (http.method = ""get"")
            } (http.baseUrl = ""/api/users"", authorize.policy = ""AuthenticatedUsers"")
        ");

        var root = ParseOutput(result);

        Assert.Equal("3.0.3", root["openapi"]!.GetValue<string>());
        Assert.Equal("ContractForge API", root["info"]!["title"]!.GetValue<string>());

        var paths = root["paths"]!.AsObject();
        var getUser = paths["/api/users/user-service-service/get-user"]!["get"]!.AsObject();
        Assert.Equal("UserService_getUser", getUser["operationId"]!.GetValue<string>());
        Assert.Equal("AuthenticatedUsers", getUser["x-authorize-policy"]!.GetValue<string>());

        var idParameter = FindParameter(getUser["parameters"]!.AsArray(), "id");
        Assert.True(idParameter["required"]!.GetValue<bool>());
        Assert.Equal("integer", idParameter["schema"]!["type"]!.GetValue<string>());
        Assert.Equal("int64", idParameter["schema"]!["format"]!.GetValue<string>());

        var searchUsers = paths["/api/users/user-service-service/search-users"]!["post"]!.AsObject();
        Assert.Equal("#/components/schemas/SearchRequest", searchUsers["requestBody"]!["content"]!["application/json"]!["schema"]!["$ref"]!.GetValue<string>());

        var searchResponses = searchUsers["responses"]!.AsObject();
        var successContent = searchResponses["200"]!["content"]!.AsObject();
        Assert.True(successContent.ContainsKey("application/json"));
        Assert.True(successContent.ContainsKey("application/x-ndjson"));
        Assert.Equal("#/components/schemas/User", successContent["application/x-ndjson"]!["schema"]!["$ref"]!.GetValue<string>());

        var exceptionSchemas = searchResponses["default"]!["content"]!["application/json"]!["schema"]!["oneOf"]!.AsArray();
        Assert.Single(exceptionSchemas);
        Assert.Equal("#/components/schemas/ValidationError", exceptionSchemas[0]!["$ref"]!.GetValue<string>());

        var deleteUserResponses = paths["/api/users/user-service-service/delete-user"]!["get"]!["responses"]!.AsObject();
        Assert.True(deleteUserResponses.ContainsKey("204"));

        var userSchema = root["components"]!["schemas"]!["User"]!.AsObject();
        Assert.True(userSchema["properties"]!["name"]!["nullable"]!.GetValue<bool>());
    }

    [Fact]
    public void Generate_WithOpenApi31Options_UsesJsonSchemaNullableAndMetadata()
    {
        var options = new OpenApiCodeGenOptions(new Dictionary<string, string>
        {
            ["openapi"] = "3.1.0",
            ["info.title"] = "Users API",
            ["info.version"] = "2.1.0",
            ["servers.0.url"] = "https://api.example.test"
        });

        var result = GenerateOpenApi(@"
            struct User {
                1: optional string name
            }

            service UserService {
                User getUser()
            }
        ", options);

        var root = ParseOutput(result);

        Assert.Equal("3.1.0", root["openapi"]!.GetValue<string>());
        Assert.Equal("Users API", root["info"]!["title"]!.GetValue<string>());
        Assert.Equal("2.1.0", root["info"]!["version"]!.GetValue<string>());
        Assert.Equal("https://api.example.test", root["servers"]![0]!["url"]!.GetValue<string>());

        var nameType = root["components"]!["schemas"]!["User"]!["properties"]!["name"]!["type"]!.AsArray();
        Assert.Equal(new[] { "string", "null" }, nameType.Select(value => value!.GetValue<string>()).ToArray());
    }

    [Fact]
    public void Generate_PostWithMultipleParameters_ReturnsError()
    {
        var result = GenerateOpenApi(@"
            service TextService {
                string combine(string first, string second)
            }
        ");

        var error = Assert.Single(result.Errors);
        Assert.Contains("POST method 'combine' has 2 parameters", error.Message);
    }

    [Fact]
    public void Generate_GetStructParameter_FlattensDottedQueryParameters()
    {
        var result = GenerateOpenApi(@"
            struct Address {
                1: required string city
            }

            struct UserFilter {
                1: required string name,
                2: optional Address address,
                3: optional list<string> tags
            }

            service UserService {
                list<string> search(UserFilter filter) (http.method = ""get"")
            }
        ");

        var root = ParseOutput(result);
        var operation = root["paths"]!["/rpc/user-service-service/search"]!["get"]!.AsObject();
        var parameters = operation["parameters"]!.AsArray();

        var name = FindParameter(parameters, "filter.name");
        var city = FindParameter(parameters, "filter.address.city");
        var tags = FindParameter(parameters, "filter.tags");

        Assert.True(name["required"]!.GetValue<bool>());
        Assert.False(city["required"]!.GetValue<bool>());
        Assert.Equal("array", tags["schema"]!["type"]!.GetValue<string>());
        Assert.Equal("form", tags["style"]!.GetValue<string>());
        Assert.False(tags["explode"]!.GetValue<bool>());
    }

    private static JsonObject ParseOutput(CodeGenResult result)
    {
        Assert.Empty(result.Errors);
        return JsonNode.Parse(result.Output)!.AsObject();
    }

    private static JsonObject FindParameter(JsonArray parameters, string name)
    {
        return parameters
            .Select(parameter => parameter!.AsObject())
            .Single(parameter => parameter["name"]!.GetValue<string>() == name);
    }
}