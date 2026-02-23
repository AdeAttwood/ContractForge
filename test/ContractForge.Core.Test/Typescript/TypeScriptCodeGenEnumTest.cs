using ContractForge.Core.Test.TestHelpers;

namespace ContractForge.Core.Test.Typescript;

public class TypeScriptCodeGenEnumTest : CodeGenTestBase
{
    [Fact]
    public Task Generate_BasicEnum_ProducesConstAndTypeAlias()
    {
        var result = GenerateTypeScript(@"
            namespace ts Test

            enum Status {
                Pending = 1,
                Active = 2,
                Completed = 3
            }
        ");

        Assert.Empty(result.Errors);
        return Verify(result.Output).UseDirectory("Snapshots");
    }

    [Fact]
    public Task Generate_EnumAsFieldType_ProducesEnumReference()
    {
        var result = GenerateTypeScript(@"
            namespace ts Test

            enum Priority {
                Low = 0,
                High = 1
            }

            struct Task {
                1: required string title,
                2: required Priority priority
            }
        ");

        Assert.Empty(result.Errors);
        return Verify(result.Output).UseDirectory("Snapshots");
    }

    [Fact]
    public Task Generate_EnumWithUnderscoreValue_UsesCamelCaseStringValue()
    {
        var result = GenerateTypeScript(@"
            namespace ts Test

            enum Progress {
                NearlyDone = 1
            }
        ");

        Assert.Empty(result.Errors);
        return Verify(result.Output).UseDirectory("Snapshots");
    }
}