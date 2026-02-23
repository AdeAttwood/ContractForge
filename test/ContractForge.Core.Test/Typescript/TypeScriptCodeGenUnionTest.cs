using ContractForge.Core.Test.TestHelpers;

namespace ContractForge.Core.Test.Typescript;

public class TypeScriptCodeGenUnionTest : CodeGenTestBase
{
    [Fact]
    public Task Generate_BasicUnion_ProducesDiscriminatedUnion()
    {
        var result = GenerateTypeScript(@"
            namespace ts Test

            union Result {
                string success,
                string error
            }
        ");

        Assert.Empty(result.Errors);
        return Verify(result.Output).UseDirectory("Snapshots");
    }

    [Fact]
    public Task Generate_UnionWithManyVariants_ProducesValidTypeScript()
    {
        var result = GenerateTypeScript(@"
            namespace ts Test

            struct ValidationError {
                1: required string message
            }

            exception NetworkError {
                1: required i32 code,
                2: required string message
            }

            union OperationResult {
                string success,
                ValidationError validationError,
                NetworkError networkError,
                string timeout
            }
        ");

        Assert.Empty(result.Errors);
        return Verify(result.Output).UseDirectory("Snapshots");
    }
}