using ContractForge.Core.Test.TestHelpers;

namespace ContractForge.Core.Test.CSharp;

public class CSharpCodeGenUnionTest : CodeGenTestBase
{
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