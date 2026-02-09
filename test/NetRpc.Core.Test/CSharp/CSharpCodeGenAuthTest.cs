using NetRpc.Core.Test.TestHelpers;

using VerifyXunit;

using Xunit;

namespace NetRpc.Core.Test.CSharp;

public class CSharpCodeGenAuthTest : CodeGenTestBase
{
    [Fact]
    public Task Generate_ServiceWithAuthPolicy_GeneratesAuthorizeAttribute()
    {
        var result = GenerateCSharp(@"
            namespace cs Test
            
            service SecureService {
                i32 sensitiveOp() (authorize.policy = ""AtLeast21"")
            } (authorize.policy = ""AuthenticatedUsers"")
        ");

        Assert.Empty(result.Errors);
        Assert.Contains("[Authorize(Policy = \"AuthenticatedUsers\")]", result.Output);
        Assert.Contains("[Authorize(Policy = \"AtLeast21\")]", result.Output);
        return Verify(result.Output).UseDirectory("Snapshots");
    }

    [Fact]
    public Task Generate_ServiceWithMultipleAuthPolicies_GeneratesAuthorizeAttributes()
    {
        var result = GenerateCSharp(@"
            namespace cs Test
            
            service MultiPolicyService {
                i32 complexOp() (authorize.policy = ""AtLeast21, CanEditProducts"")
            }
        ");

        Assert.Empty(result.Errors);
        Assert.Contains("[Authorize(Policy = \"AtLeast21\")]", result.Output);
        Assert.Contains("[Authorize(Policy = \"CanEditProducts\")]", result.Output);
        return Verify(result.Output).UseDirectory("Snapshots");
    }

    [Fact]
    public Task Generate_ServiceWithMessyAuthPolicies_GeneratesCorrectAuthorizeAttributes()
    {
        var result = GenerateCSharp(@"
            namespace cs Test
            
            service MessyPolicyService {
                i32 messyOp() (authorize.policy = "" A , B, , C "")
            }
        ");

        Assert.Empty(result.Errors);
        Assert.Contains("[Authorize(Policy = \"A\")]", result.Output);
        Assert.Contains("[Authorize(Policy = \"B\")]", result.Output);
        Assert.Contains("[Authorize(Policy = \"C\")]", result.Output);
        // Ensure no empty policy
        Assert.DoesNotContain("[Authorize(Policy = \"\")]", result.Output);
        Assert.DoesNotContain("[Authorize(Policy = \" \")]", result.Output);

        return Verify(result.Output).UseDirectory("Snapshots");
    }
}