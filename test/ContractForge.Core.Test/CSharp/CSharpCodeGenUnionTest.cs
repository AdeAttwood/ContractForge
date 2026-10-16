using ContractForge.Core.Test.TestHelpers;

namespace ContractForge.Core.Test.CSharp;

public class CSharpCodeGenUnionTest : CodeGenTestBase
{
    [Fact]
    public async Task GeneratedUnion_DeserializesScalarBranchesAndValidatesWireShape()
    {
        var result = GenerateCSharp("""
            namespace cs Test

            struct ChoiceData {
                1: required i32 value
            }

            union Choice {
                1: i32 count,
                2: ChoiceData data
            }
            """);

        Assert.Empty(result.Errors);
        Assert.Contains("[JsonConverter(typeof(ChoiceJsonConverter))]", result.Output);

        await CompileAndRun(result.Output, """
            using System;
            using System.Text.Json;
            using System.Text.Json.Serialization;
            using System.Threading.Tasks;
            using Test;

            namespace RuntimeTests;

            public static class GeneratedUnionTests
            {
                public static Task Run()
                {
                    var defaultJson = JsonSerializer.Serialize(new Choice(7));
                    Equal("{\"type\":\"count\",\"count\":7}", defaultJson);
                    var defaultValue = JsonSerializer.Deserialize<Choice>(defaultJson)!;
                    Equal(ChoiceType.Count, defaultValue.Type);
                    Equal(7, defaultValue.Count);
                    Null(defaultValue.Data);

                    var webJson = JsonSerializer.Serialize(
                        new Choice(new ChoiceData { Value = 9 }),
                        JsonSerializerOptions.Web);
                    Equal("{\"type\":\"data\",\"data\":{\"value\":9}}", webJson);
                    var webValue = JsonSerializer.Deserialize<Choice>(webJson, JsonSerializerOptions.Web)!;
                    Equal(9, webValue.Data!.Value);
                    Null(webValue.Count);

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        NumberHandling = JsonNumberHandling.AllowReadingFromString
                    };
                    var reordered = JsonSerializer.Deserialize<Choice>(
                        "{\"DATA\":{\"VALUE\":\"42\"},\"ignored\":true,\"TYPE\":\"data\"}",
                        options)!;
                    Equal(42, reordered.Data!.Value);

                    Throws("{}");
                    Throws("{\"type\":\"unknown\",\"count\":1}");
                    Throws("{\"type\":\"count\"}");
                    Throws("{\"type\":\"count\",\"count\":null}");
                    Throws("{\"type\":\"count\",\"count\":1,\"data\":{\"value\":2}}");
                    Throws("{\"type\":\"count\",\"data\":{\"value\":2}}");
                    return Task.CompletedTask;
                }

                private static void Throws(string json)
                {
                    try
                    {
                        JsonSerializer.Deserialize<Choice>(json);
                        throw new InvalidOperationException($"Expected JsonException for {json}");
                    }
                    catch (JsonException)
                    {
                    }
                }

                private static void Equal<T>(T expected, T actual)
                {
                    if (!Equals(expected, actual))
                    {
                        throw new InvalidOperationException($"Expected {expected}, got {actual}.");
                    }
                }

                private static void Null(object? value)
                {
                    if (value is not null)
                    {
                        throw new InvalidOperationException("Expected null.");
                    }
                }
            }
            """);
    }

    [Fact]
    public async Task GeneratedResponseUnions_WorkWithHttpClientAndIncrementalArrays()
    {
        var result = GenerateCSharp("""
            namespace cs Test

            struct Execution {
                1: required string id
            }

            struct OutputPage {
                1: required list<string> chunks
            }

            struct ExecutionError {
                1: required string code,
                2: required string message
            }

            union ExecutionResult {
                1: Execution execution,
                2: ExecutionError error
            }

            union OutputResult {
                1: OutputPage output,
                2: ExecutionError error
            }
            """);

        Assert.Empty(result.Errors);

        await CompileAndRun(result.Output, """
            using System;
            using System.Collections.Generic;
            using System.IO;
            using System.Net;
            using System.Net.Http;
            using System.Net.Http.Json;
            using System.Text;
            using System.Text.Json;
            using System.Threading;
            using System.Threading.Tasks;
            using Test;

            namespace RuntimeTests;

            public static class GeneratedUnionTests
            {
                public static async Task Run()
                {
                    var responses = new Queue<string>([
                        "{\"execution\":{\"id\":\"run-1\"},\"type\":\"execution\"}",
                        "{\"type\":\"error\",\"error\":{\"code\":\"not-found\",\"message\":\"missing\"}}",
                        "{\"type\":\"output\",\"output\":{\"chunks\":[\"one\",\"two\"]}}",
                        "{\"error\":{\"code\":\"ended\",\"message\":\"done\"},\"type\":\"error\"}"
                    ]);
                    using var client = new HttpClient(new QueueHandler(responses))
                    {
                        BaseAddress = new Uri("https://example.test")
                    };

                    var execution = await client.GetFromJsonAsync<ExecutionResult>("execution");
                    Equal("run-1", execution!.Execution!.Id);
                    var executionError = await client.GetFromJsonAsync<ExecutionResult>("execution-error");
                    Equal("not-found", executionError!.Error!.Code);
                    var output = await client.GetFromJsonAsync<OutputResult>("output");
                    Equal("one", First(output!.Output!.Chunks));
                    var outputError = await client.GetFromJsonAsync<OutputResult>("output-error");
                    Equal("ended", outputError!.Error!.Code);

                    const string arrayJson = "["
                        + "{\"type\":\"output\",\"output\":{\"chunks\":[\"first\"]}},"
                        + "{\"error\":{\"code\":\"failed\",\"message\":\"stopped\"},\"type\":\"error\"}"
                        + "]";
                    await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(arrayJson));
                    var items = new List<OutputResult>();
                    await foreach (var item in JsonSerializer.DeserializeAsyncEnumerable<OutputResult>(
                        stream,
                        JsonSerializerOptions.Web))
                    {
                        items.Add(item!);
                    }

                    Equal(2, items.Count);
                    Equal("first", First(items[0].Output!.Chunks));
                    Equal("failed", items[1].Error!.Code);
                }

                private static string First(IEnumerable<string> values)
                {
                    foreach (var value in values)
                    {
                        return value;
                    }
                    throw new InvalidOperationException("Expected an item.");
                }

                private static void Equal<T>(T expected, T actual)
                {
                    if (!Equals(expected, actual))
                    {
                        throw new InvalidOperationException($"Expected {expected}, got {actual}.");
                    }
                }

                private sealed class QueueHandler(Queue<string> responses) : HttpMessageHandler
                {
                    protected override Task<HttpResponseMessage> SendAsync(
                        HttpRequestMessage request,
                        CancellationToken cancellationToken)
                    {
                        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                        {
                            Content = new StringContent(responses.Dequeue(), Encoding.UTF8, "application/json")
                        });
                    }
                }
            }
            """);
    }

    [Fact]
    public Task Generate_BasicUnion_ProducesValidCSharpCode()
    {
        var result = GenerateCSharp(@"
            namespace cs Test
            
            union Result {
                1: string success,
                2: string error
            }
        ");

        Assert.Empty(result.Errors);
        return Verify(result.Output).UseDirectory("Snapshots");
    }

    [Fact]
    public Task Generate_UnionWithStructs_ProducesValidCSharpCode()
    {
        var result = GenerateCSharp(@"
            namespace cs Test
            
            struct SuccessData {
                1: required i64 id,
                2: required string message
            }
            
            struct ErrorData {
                1: required i32 code,
                2: required string details
            }
            
            union OperationResult {
                1: SuccessData success,
                2: ErrorData error
            }
        ");

        Assert.Empty(result.Errors);
        return Verify(result.Output).UseDirectory("Snapshots");
    }

    [Fact]
    public Task Generate_UnionWithException_ProducesValidCSharpCode()
    {
        var result = GenerateCSharp(@"
            namespace cs Test
            
            exception GenericError {
                1: required i32 code,
                2: required string message
            }
            
            union NumberResult {
                1: i64 success,
                2: GenericError error
            }
        ");

        Assert.Empty(result.Errors);
        return Verify(result.Output).UseDirectory("Snapshots");
    }

    [Fact]
    public Task Generate_UnionWithManyVariants_ProducesValidCSharpCode()
    {
        var result = GenerateCSharp(@"
            namespace cs Test
            
            union StatusResult {
                1: string pending,
                2: string inProgress,
                3: string completed,
                4: string failed,
                5: string cancelled
            }
        ");

        Assert.Empty(result.Errors);
        return Verify(result.Output).UseDirectory("Snapshots");
    }

    [Fact]
    public Task Generate_UnionAsFieldType_ProducesValidCSharpCode()
    {
        var result = GenerateCSharp(@"
            namespace cs Test
            
            union Status {
                1: string active,
                2: string inactive
            }
            
            struct User {
                1: required string name,
                2: required Status status
            }
        ");

        Assert.Empty(result.Errors);
        return Verify(result.Output).UseDirectory("Snapshots");
    }

    [Fact]
    public Task Generate_UnionAsReturnType_ProducesValidCSharpCode()
    {
        var result = GenerateCSharp(@"
            namespace cs Test
            
            union Result {
                1: i64 success,
                2: string error
            }
            
            service CalculatorService {
                Result calculate(i32 a, i32 b) (http.method = ""get"")
            }
        ");

        Assert.Empty(result.Errors);
        return Verify(result.Output).UseDirectory("Snapshots");
    }

    [Fact]
    public Task Generate_UnionWithDocumentation_IncludesXmlComments()
    {
        var result = GenerateCSharp(@"
            namespace cs Test
            
            /**
             * Represents the result of an operation
             */
            union OperationResult {
                /**
                 * Success case with value
                 */
                1: i64 success,
                /**
                 * Error case with message
                 */
                2: string error
            }
        ");

        Assert.Empty(result.Errors);
        return Verify(result.Output).UseDirectory("Snapshots");
    }

    [Fact]
    public Task Generate_UnionWithNamespace_ProducesIndentedCode()
    {
        var result = GenerateCSharp(@"
            namespace cs Test.Models
            
            union Result {
                1: string success,
                2: string error
            }
        ");

        Assert.Empty(result.Errors);
        Assert.Contains("namespace Test.Models", result.Output);
        return Verify(result.Output).UseDirectory("Snapshots");
    }
}