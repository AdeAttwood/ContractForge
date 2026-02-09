using NetRpc.Core.Test.TestHelpers;

namespace NetRpc.Core.Test.CSharp;

public class CSharpCodeGenPostValidationTest : CodeGenTestBase
{
    [Fact]
    public void Generate_PostMethodWithTwoParameters_ProducesError()
    {
        var result = GenerateCSharp(@"
            namespace cs Test
            
            service MyService {
                void doThing(string a, string b)
            }
        ");

        Assert.Single(result.Errors);
        Assert.Contains("POST", result.Errors[0].Message);
        Assert.Contains("multiple parameters", result.Errors[0].Message);
    }

    [Fact]
    public void Generate_PostMethodWithThreeParameters_ProducesError()
    {
        var result = GenerateCSharp(@"
            namespace cs Test
            
            service MyService {
                string process(string a, i32 b, bool c)
            }
        ");

        Assert.Single(result.Errors);
        Assert.Contains("POST", result.Errors[0].Message);
        Assert.Contains("multiple parameters", result.Errors[0].Message);
    }

    [Fact]
    public Task Generate_PostMethodWithZeroParameters_ProducesValidCode()
    {
        var result = GenerateCSharp(@"
            namespace cs Test
            
            service MyService {
                string getData()
            }
        ");

        Assert.Empty(result.Errors);
        return Verify(result.Output).UseDirectory("Snapshots");
    }

    [Fact]
    public Task Generate_PostMethodWithOneParameter_ProducesValidCode()
    {
        var result = GenerateCSharp(@"
            namespace cs Test
            
            struct Request {
                1: required string name
            }
            
            service MyService {
                string process(Request request)
            }
        ");

        Assert.Empty(result.Errors);
        return Verify(result.Output).UseDirectory("Snapshots");
    }

    [Fact]
    public Task Generate_GetMethodWithMultipleParameters_ProducesValidCode()
    {
        var result = GenerateCSharp(@"
            namespace cs Test
            
            service CalculatorService {
                i32 add(i32 a, i32 b) (http.method = ""get"")
            }
        ");

        Assert.Empty(result.Errors);
        return Verify(result.Output).UseDirectory("Snapshots");
    }

    [Fact]
    public void Generate_MixedMethods_ProducesErrorsOnlyForInvalidPosts()
    {
        var result = GenerateCSharp(@"
            namespace cs Test
            
            struct Request {
                1: required string data
            }
            
            service MyService {
                string process(Request request),
                i32 calculate(i32 a, i32 b) (http.method = ""get""),
                void doThing(string a, string b),
                string getData()
            }
        ");

        // Should have exactly 1 error for the invalid POST method
        Assert.Single(result.Errors);
        Assert.Contains("POST", result.Errors[0].Message);
        Assert.Contains("multiple parameters", result.Errors[0].Message);
    }
}