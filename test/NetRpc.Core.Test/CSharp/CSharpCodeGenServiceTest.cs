using NetRpc.Core.Test.TestHelpers;

namespace NetRpc.Core.Test.CSharp;

public class CSharpCodeGenServiceTest : CodeGenTestBase
{
    [Fact]
    public Task Generate_BasicService_ProducesInterfaceAndController()
    {
        var result = GenerateCSharp(@"
            namespace cs Test
            
            service CalculatorService {
                i32 add(i32 a, i32 b) (http.method = ""get""),
                i32 subtract(i32 a, i32 b) (http.method = ""get"")
            }
        ");

        Assert.Empty(result.Errors);
        Assert.Contains("interface ICalculatorServiceService", result.Output);
        Assert.Contains("class CalculatorServiceBaseController", result.Output);
        return Verify(result.Output).UseDirectory("Snapshots");
    }

    [Fact]
    public Task Generate_ServiceWithDocumentation_IncludesXmlComments()
    {
        var result = GenerateCSharp(@"
            namespace cs Test
            
            /**
             * Service for performing calculations
             */
            service CalculatorService {
                /**
                 * Adds two numbers together
                 */
                i32 add(i32 a, i32 b) (http.method = ""get"")
            }
        ");

        Assert.Empty(result.Errors);
        Assert.Contains("/// <summary>", result.Output);
        Assert.Contains("Service for performing calculations", result.Output);
        Assert.Contains("Adds two numbers together", result.Output);
        return Verify(result.Output).UseDirectory("Snapshots");
    }

    [Fact]
    public Task Generate_ServiceWithPostMethod_GeneratesFromBodyAttribute()
    {
        var result = GenerateCSharp(@"
            namespace cs Test
            
            struct Request {
                1: required string name
            }
            
            service UserService {
                string process(Request request)
            }
        ");

        Assert.Empty(result.Errors);
        Assert.Contains("[FromBody]", result.Output);
        return Verify(result.Output).UseDirectory("Snapshots");
    }

    [Fact]
    public Task Generate_ServiceWithGetMethod_GeneratesFromQueryAttributes()
    {
        var result = GenerateCSharp(@"
            namespace cs Test
            
            service CalculatorService {
                i32 add(i32 a, i32 b) (http.method = ""get"")
            }
        ");

        Assert.Empty(result.Errors);
        Assert.Contains("[FromQuery]", result.Output);
        return Verify(result.Output).UseDirectory("Snapshots");
    }

    [Fact]
    public Task Generate_ServiceWithStreamingReturn_GeneratesIAsyncEnumerable()
    {
        var result = GenerateCSharp(@"
            namespace cs Test
            
            service DataService {
                list<i64> getNumbers(i32 start, i32 end) (http.method = ""get"")
            }
        ");

        Assert.Empty(result.Errors);
        Assert.Contains("IAsyncEnumerable<", result.Output);
        return Verify(result.Output).UseDirectory("Snapshots");
    }

    [Fact]
    public Task Generate_ServiceWithCancellationToken_IncludesParameter()
    {
        var result = GenerateCSharp(@"
            namespace cs Test
            
            service SimpleService {
                string echo(string message)
            }
        ");

        Assert.Empty(result.Errors);
        Assert.Contains("CancellationToken cancellationToken", result.Output);
        return Verify(result.Output).UseDirectory("Snapshots");
    }

    [Fact]
    public Task Generate_EmptyService_ProducesEmptyInterfaceAndController()
    {
        var result = GenerateCSharp(@"
            namespace cs Test
            
            service EmptyService {
            }
        ");

        Assert.Empty(result.Errors);
        Assert.Contains("interface IEmptyServiceService", result.Output);
        Assert.Contains("class EmptyServiceBaseController", result.Output);
        return Verify(result.Output).UseDirectory("Snapshots");
    }
}