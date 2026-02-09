using NetRpc.Core.Test.TestHelpers;

namespace NetRpc.Core.Test.CSharp;

public class CSharpCodeGenDocumentationTest : CodeGenTestBase
{
    [Fact]
    public Task Generate_WithDocComments_ProducesXmlDocComments()
    {
        var result = GenerateCSharp(@"
            namespace cs Test

            /** 
             * Represents a user profile in the system.
             * Contains personal information and settings.
             */
            struct User {
                /** The user's unique identifier */
                1: required string id,
                /** The user's full name */
                2: required string name,
                /** The user's age in years */
                3: required i32 age
            }

            /**
             * Thrown when validation fails.
             */
            exception ValidationError {
                /** Error code for categorization */
                1: required i32 code,
                /** Human-readable error message */
                2: required string message
            }

            /**
             * Result of a user operation.
             */
            union UserResult {
                /** Operation succeeded with user data */
                User success,
                /** Operation failed with validation error */
                ValidationError error
            }

            /**
             * Service for managing user accounts.
             */
            service UserService {
                /**
                 * Creates a new user account.
                 * Returns the created user or validation error.
                 */
                UserResult createUser(User user),
                
                /**
                 * Retrieves a user by their unique ID.
                 * Returns the user if found, or error if not.
                 */
                UserResult getUser(string id) (http.method = ""get"")
            }
        ");

        Assert.Empty(result.Errors);
        return Verify(result.Output).UseDirectory("Snapshots");
    }

    [Fact]
    public Task Generate_WithoutDocComments_ProducesNoXmlComments()
    {
        var result = GenerateCSharp(@"
            namespace cs Test

            struct User {
                1: required string id,
                2: required string name,
                3: required i32 age
            }

            exception ValidationError {
                1: required i32 code,
                2: required string message
            }

            union UserResult {
                User success,
                ValidationError error
            }

            service UserService {
                UserResult createUser(User user),
                UserResult getUser(string id) (http.method = ""get"")
            }
        ");

        Assert.Empty(result.Errors);
        return Verify(result.Output).UseDirectory("Snapshots");
    }
}