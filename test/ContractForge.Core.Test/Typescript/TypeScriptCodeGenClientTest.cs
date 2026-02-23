using ContractForge.Core.Test.TestHelpers;

namespace ContractForge.Core.Test.Typescript;

public class TypeScriptCodeGenClientTest : CodeGenTestBase
{
    [Fact]
    public Task Generate_ServiceWithSingleParameter_ProducesPostMethod()
    {
        var result = GenerateTypeScript(@"
            namespace ts Test
            
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
    public Task Generate_ServiceWithMultipleParameters_ProducesGetMethod()
    {
        var result = GenerateTypeScript(@"
            namespace ts Test
            
            service CalculatorService {
                i32 add(i32 a, i32 b)
            }
        ");

        Assert.Empty(result.Errors);
        return Verify(result.Output).UseDirectory("Snapshots");
    }

    [Fact]
    public Task Generate_ServiceWithGetAnnotation_ProducesGetMethod()
    {
        var result = GenerateTypeScript(@"
            namespace ts Test
            
            service DataService {
                string getData(i32 id, string filter) (http.method = ""get"")
            }
        ");

        Assert.Empty(result.Errors);
        return Verify(result.Output).UseDirectory("Snapshots");
    }

    [Fact]
    public Task Generate_ServiceWithStreamingReturn_ProducesStreamMethod()
    {
        var result = GenerateTypeScript(@"
            namespace ts Test
            
            service DataService {
                list<i64> getNumbers(i32 start, i32 end)
            }
        ");

        Assert.Empty(result.Errors);
        return Verify(result.Output).UseDirectory("Snapshots");
    }

    [Fact]
    public Task Generate_ServiceWithStringReturn_ProducesMethod()
    {
        var result = GenerateTypeScript(@"
            namespace ts Test
            
            struct Request {
                1: required string data
            }
            
            service ActionService {
                string execute(Request request)
            }
        ");

        Assert.Empty(result.Errors);
        return Verify(result.Output).UseDirectory("Snapshots");
    }

    [Fact]
    public Task Generate_ServiceWithDocumentation_IncludesJsDocComments()
    {
        var result = GenerateTypeScript(@"
            namespace ts Test
            
            /**
             * A simple calculator service
             */
            service CalculatorService {
                /**
                 * Adds two numbers together
                 */
                i32 add(i32 a, i32 b)
            }
        ");

        Assert.Empty(result.Errors);
        return Verify(result.Output).UseDirectory("Snapshots");
    }

    [Fact]
    public Task Generate_ClientWithFlattenUtility_IncludesHelperFunction()
    {
        var result = GenerateTypeScript(@"
            namespace ts Test
            
            service MyService {
                string getData(i32 id, string filter)
            }
        ");

        Assert.Empty(result.Errors);
        Assert.Contains("function flatten", result.Output);
        return Verify(result.Output).UseDirectory("Snapshots");
    }

    [Fact]
    public Task Generate_ClientWithConstructor_IncludesOptionsParameter()
    {
        var result = GenerateTypeScript(@"
            namespace ts Test
            
            service MyService {
                string doSomething()
            }
        ");

        Assert.Empty(result.Errors);
        Assert.Contains("constructor", result.Output);
        return Verify(result.Output).UseDirectory("Snapshots");
    }

    [Fact]
    public Task Generate_ClientWithPrivateMethods_IncludesRequestHelper()
    {
        var result = GenerateTypeScript(@"
            namespace ts Test
            
            service MyService {
                string getData()
            }
        ");

        Assert.Empty(result.Errors);
        Assert.Contains("private", result.Output);
        Assert.Contains("request", result.Output);
        return Verify(result.Output).UseDirectory("Snapshots");
    }

    [Fact]
    public Task Generate_ClientWithPrivateMethods_IncludesStreamNdjsonHelper()
    {
        var result = GenerateTypeScript(@"
            namespace ts Test
            
            service DataService {
                list<string> streamData()
            }
        ");

        Assert.Empty(result.Errors);
        Assert.Contains("streamNdjson", result.Output);
        return Verify(result.Output).UseDirectory("Snapshots");
    }

    [Fact]
    public Task Generate_ServiceWithUnionReturn_ProducesCorrectReturnType()
    {
        var result = GenerateTypeScript(@"
            namespace ts Test
            
            exception ValidationError {
                1: required i32 code,
                2: required string message
            }
            
            union Result {
                1: i64 success,
                2: ValidationError error
            }
            
            service MyService {
                Result process()
            }
        ");

        Assert.Empty(result.Errors);
        return Verify(result.Output).UseDirectory("Snapshots");
    }

    [Fact]
    public Task Generate_ServiceWithExceptionThrows_IncludesExceptionInReturnType()
    {
        var result = GenerateTypeScript(@"
            namespace ts Test
            
            exception ValidationError {
                1: required i32 code,
                2: required string message
            }
            
            service MyService {
                string validate() throws (1: ValidationError error)
            }
        ");

        Assert.Empty(result.Errors);
        return Verify(result.Output).UseDirectory("Snapshots");
    }

    [Fact]
    public Task Generate_EmptyService_ProducesEmptyClient()
    {
        var result = GenerateTypeScript(@"
            namespace ts Test
            
            service EmptyService {
            }
        ");

        Assert.Empty(result.Errors);
        return Verify(result.Output).UseDirectory("Snapshots");
    }
}