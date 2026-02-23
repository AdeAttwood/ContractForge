using ContractForge.Core.Test.TestHelpers;

namespace ContractForge.Core.Test.CSharp;

public class CSharpCodeGenNamespaceTest : CodeGenTestBase
{
    [Fact]
    public Task Generate_WithNamespace_ProducesIndentedCode()
    {
        var result = GenerateCSharp(@"
            namespace cs Test.Models

            struct User {
                1: required string name,
                2: required i32 age
            }
        ");

        Assert.Empty(result.Errors);
        return Verify(result.Output).UseDirectory("Snapshots");
    }

    [Fact]
    public Task Generate_WithoutNamespace_ProducesUnindentedCode()
    {
        var result = GenerateCSharp(@"
            struct User {
                1: required string name,
                2: required i32 age
            }
        ");

        Assert.Empty(result.Errors);
        return Verify(result.Output).UseDirectory("Snapshots");
    }

    [Fact]
    public Task Generate_WithEmptyNamespace_ProducesUnindentedCode()
    {
        var result = GenerateCSharp(@"
            namespace cs

            struct User {
                1: required string name,
                2: required i32 age
            }
        ");

        Assert.Empty(result.Errors);
        return Verify(result.Output).UseDirectory("Snapshots");
    }

    [Fact]
    public Task Generate_ComplexStructWithNamespace_ProducesConsistentIndentation()
    {
        var result = GenerateCSharp(@"
            namespace cs MyApp.Domain

            struct Address {
                1: required string street,
                2: required string city,
                3: required string state,
                4: required string zipCode
            }

            struct User {
                1: required string name,
                2: required i32 age,
                3: optional string email,
                4: required Address address,
                5: required list<string> phoneNumbers,
                6: required map<string, string> metadata
            }

            exception ValidationError {
                1: required i32 code,
                2: required string message
            }

            union Result {
                User success,
                ValidationError error
            }

            service UserService {
                Result createUser(User user),
                Result getUser(string id) (http.method = ""get"")
            }
        ");

        Assert.Empty(result.Errors);
        return Verify(result.Output).UseDirectory("Snapshots");
    }
}