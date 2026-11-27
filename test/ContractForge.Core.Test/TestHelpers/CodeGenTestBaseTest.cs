using Xunit.Sdk;

namespace ContractForge.Core.Test.TestHelpers;

public class CodeGenTestBaseTest : CodeGenTestBase
{
    [Fact]
    public void Generate_WithParserDiagnostic_FailsForEveryGenerator()
    {
        const string thrift = """
            namespace cs Test
            ,
            service Greeter {
                string greet()
            }
            """;

        AssertAllGeneratorsReject(thrift, "Syntax error");
    }

    [Fact]
    public void Generate_WithLinterDiagnostic_FailsForEveryGenerator()
    {
        const string thrift = """
            namespace cs Test

            service Greeter {
                string greet()
                string farewell()
            }
            """;

        AssertAllGeneratorsReject(thrift, "Missing comma");
    }

    [Fact]
    public void Generate_WithValidThrift_ReturnsGeneratorResults()
    {
        const string thrift = """
            namespace cs Test

            service Greeter {
                string greet()
            }
            """;

        var results = new[]
        {
            GenerateCSharp(thrift),
            GenerateTypeScript(thrift),
            GenerateOpenApi(thrift)
        };

        Assert.All(results, result =>
        {
            Assert.Empty(result.Errors);
            Assert.False(string.IsNullOrWhiteSpace(result.Output));
        });
    }

    private void AssertAllGeneratorsReject(string thrift, string expectedDiagnostic)
    {
        var exceptions = new[]
        {
            Assert.ThrowsAny<XunitException>(() => GenerateCSharp(thrift)),
            Assert.ThrowsAny<XunitException>(() => GenerateTypeScript(thrift)),
            Assert.ThrowsAny<XunitException>(() => GenerateOpenApi(thrift))
        };

        Assert.All(exceptions, exception =>
        {
            Assert.Contains("parser/linter diagnostics", exception.Message);
            Assert.Contains(expectedDiagnostic, exception.Message);
        });
    }
}