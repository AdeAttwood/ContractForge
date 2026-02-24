using ContractForge.Core.CSharp;
using ContractForge.Core.Test.TestHelpers;

namespace ContractForge.Core.Test.CSharp;

public class CSharpCodeGenOptionsTest : CodeGenTestBase
{
    [Fact]
    public void Constructor_WithUnsupportedAlias_Throws()
    {
        Assert.Throws<ArgumentException>(() => new CSharpCodeGenOptions(new Dictionary<string, string>
        {
            { "csharp.partial", "true" }
        }));
    }

    [Fact]
    public void Generate_WithPartialClasses_EmitsPartialClassesForDtos()
    {
        var result = GenerateCSharp(@"
            namespace cs Test

            struct CreateProjectRequest {
                1: required string name,
                2: required string gitRepo
            }

            union Result {
                1: string success,
                2: string error
            }

            exception ValidationError {
                1: required string message
            }
        ", new CSharpCodeGenOptions { UsePartialClasses = true });

        Assert.Empty(result.Errors);
        Assert.Contains("public partial class CreateProjectRequest", result.Output);
        Assert.Contains("public enum ResultType", result.Output);
        Assert.Contains("public partial class Result", result.Output);
        Assert.Contains("public partial class ValidationError : Exception", result.Output);
    }
}