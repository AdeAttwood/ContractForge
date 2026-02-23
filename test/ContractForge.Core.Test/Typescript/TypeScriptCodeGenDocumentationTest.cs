using ContractForge.Core.Test.TestHelpers;

namespace ContractForge.Core.Test.Typescript;

public class TypeScriptCodeGenDocumentationTest : CodeGenTestBase
{
    [Fact]
    public Task Generate_WithDocComments_ProducesJsDocComments()
    {
        var result = GenerateTypeScript(@"
            namespace ts Test

            /** 
             * Represents a user profile in the system.
             * This structure contains all user-related data.
             */
            struct User {
                /** The user's unique identifier */
                1: required string id,
                /** The user's full name */
                2: required string name
            }

            /**
             * Represents an authentication error.
             */
            exception AuthError {
                /** Error code */
                1: required i32 code,
                /** Error message */
                2: required string message
            }

            /**
             * Result of an authentication operation.
             */
            union AuthResult {
                /** Successful authentication with user data */
                User success,
                /** Authentication failed with error */
                AuthError error
            }

            /**
             * Authentication service for managing user sessions.
             */
            service AuthService {
                /**
                 * Authenticates a user with their credentials.
                 * Returns user data on success or error on failure.
                 */
                AuthResult login(string username, string password) (http.method = ""get"")
            }
        ");

        Assert.Empty(result.Errors);
        return Verify(result.Output).UseDirectory("Snapshots");
    }
}