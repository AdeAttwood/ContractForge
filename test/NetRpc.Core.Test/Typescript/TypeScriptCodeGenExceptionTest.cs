using NetRpc.Core.Test.TestHelpers;

namespace NetRpc.Core.Test.Typescript;

public class TypeScriptCodeGenExceptionTest : CodeGenTestBase
{
    [Fact]
    public Task Generate_BasicException_ProducesInterface()
    {
        var result = GenerateTypeScript(@"
            namespace ts Test

            exception ValidationError {
                1: required i32 code,
                2: required string message
            }
        ");

        Assert.Empty(result.Errors);
        return Verify(result.Output).UseDirectory("Snapshots");
    }

    [Fact]
    public Task Generate_ExceptionWithDocumentation_IncludesJsDocComments()
    {
        var result = GenerateTypeScript(@"
            namespace ts Test

            /** 
             * Represents a validation error that occurs during request processing.
             */
            exception ValidationError {
                /** Error code for categorization */
                1: required i32 code,
                /** Human-readable error message */
                2: required string message,
                /** Additional error details */
                3: optional list<string> details
            }
        ");

        Assert.Empty(result.Errors);
        return Verify(result.Output).UseDirectory("Snapshots");
    }
}