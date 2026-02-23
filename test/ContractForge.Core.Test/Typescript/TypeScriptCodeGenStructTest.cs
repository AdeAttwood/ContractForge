using ContractForge.Core.Test.TestHelpers;

namespace ContractForge.Core.Test.Typescript;

public class TypeScriptCodeGenStructTest : CodeGenTestBase
{
    [Fact]
    public Task Generate_BasicStruct_ProducesValidTypeScript()
    {
        var result = GenerateTypeScript(@"
            namespace ts Test

            struct User {
                1: required string name,
                2: required i32 age,
                3: optional string email
            }
        ");

        Assert.Empty(result.Errors);
        return Verify(result.Output).UseDirectory("Snapshots");
    }

    [Fact]
    public Task Generate_StructWithDocumentation_IncludesJsDocComments()
    {
        var result = GenerateTypeScript(@"
            namespace ts Test

            /** 
             * Represents a user in the system.
             * Contains basic profile information.
             */
            struct User {
                /** The user's full name */
                1: required string name,
                /** The user's age in years */
                2: required i32 age
            }
        ");

        Assert.Empty(result.Errors);
        return Verify(result.Output).UseDirectory("Snapshots");
    }

    [Fact]
    public Task Generate_StructWithNestedTypes_ProducesValidTypeScript()
    {
        var result = GenerateTypeScript(@"
            namespace ts Test

            struct Address {
                1: required string street,
                2: required string city
            }

            struct User {
                1: required string name,
                2: required Address address,
                3: required list<string> tags,
                4: required map<string, i32> scores
            }
        ");

        Assert.Empty(result.Errors);
        return Verify(result.Output).UseDirectory("Snapshots");
    }
}