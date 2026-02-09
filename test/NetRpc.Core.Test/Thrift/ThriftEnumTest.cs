using NetRpc.Core;
using NetRpc.Core.Thrift;
using NetRpc.Core.Types;

public class ThriftEnumTest
{
    [Fact]
    public void Load_BasicEnum_ParsesCorrectly()
    {
        var document = LoadThrift(@"
            namespace cs Test
            
            enum Status {
                PENDING = 1,
                ACTIVE = 2,
                COMPLETED = 3
            }
        ");

        Assert.Empty(document.Errors);
        Assert.Single(document.Enums);
        Assert.True(document.Enums.ContainsKey("Status"));

        var status = document.Enums["Status"];
        Assert.Equal(3, status.Values.Count);
        Assert.Equal(1, status.Values["PENDING"].Value);
        Assert.Equal(2, status.Values["ACTIVE"].Value);
        Assert.Equal(3, status.Values["COMPLETED"].Value);
    }

    [Fact]
    public void Load_EnumWithAutoIncrement_AssignsSequentialValues()
    {
        var document = LoadThrift(@"
            enum Status {
                PENDING,
                ACTIVE,
                COMPLETED
            }
        ");

        Assert.Empty(document.Errors);
        var status = document.Enums["Status"];
        Assert.Equal(0, status.Values["PENDING"].Value);
        Assert.Equal(1, status.Values["ACTIVE"].Value);
        Assert.Equal(2, status.Values["COMPLETED"].Value);
    }

    [Fact]
    public void Load_EnumWithMixedValues_HandlesCorrectly()
    {
        var document = LoadThrift(@"
            enum Status {
                PENDING = 5,
                ACTIVE,
                COMPLETED = 10
            }
        ");

        Assert.Empty(document.Errors);
        var status = document.Enums["Status"];
        Assert.Equal(5, status.Values["PENDING"].Value);
        Assert.Equal(6, status.Values["ACTIVE"].Value);
        Assert.Equal(10, status.Values["COMPLETED"].Value);
    }

    [Fact]
    public void Load_EnumWithNegativeValue_ParsesCorrectly()
    {
        var document = LoadThrift(@"
            enum Status {
                ERROR = -1,
                PENDING = 0,
                ACTIVE = 1
            }
        ");

        Assert.Empty(document.Errors);
        var status = document.Enums["Status"];
        Assert.Equal(-1, status.Values["ERROR"].Value);
        Assert.Equal(0, status.Values["PENDING"].Value);
        Assert.Equal(1, status.Values["ACTIVE"].Value);
    }

    [Fact]
    public void Load_EnumWithDocumentation_PreservesComments()
    {
        var document = LoadThrift(@"
            /** User status enum */
            enum Status {
                /** User is pending approval */
                PENDING = 1,
                /** User is active */
                ACTIVE = 2
            }
        ");

        Assert.Empty(document.Errors);
        var status = document.Enums["Status"];
        Assert.Contains("User status enum", status.Description ?? "");
        Assert.Contains("pending approval", status.Values["PENDING"].Description ?? "");
        Assert.Contains("active", status.Values["ACTIVE"].Description ?? "");
    }

    [Fact]
    public void Load_EnumAsStructField_ResolvesCorrectly()
    {
        var document = LoadThrift(@"
            enum Status {
                PENDING = 1,
                ACTIVE = 2
            }
            
            struct User {
                string name,
                Status status
            }
        ");

        Assert.Empty(document.Errors);
        Assert.Single(document.Enums);
        Assert.Single(document.Structs);

        var userStruct = document.Structs["User"];
        var statusField = userStruct.Fields["status"];

        // Verify the type resolved to the enum
        Assert.IsType<NetRpc.Core.Types.Enum>(statusField.Type);
        Assert.Equal("Status", ((NetRpc.Core.Types.Enum)statusField.Type).Identifier);
    }

    [Fact]
    public void Load_EnumAsServiceParameter_ResolvesCorrectly()
    {
        var document = LoadThrift(@"
            enum Status {
                PENDING = 1,
                ACTIVE = 2
            }
            
            service UserService {
                void updateStatus(Status newStatus)
            }
        ");

        Assert.Empty(document.Errors);
        var service = document.Services["UserService"];
        var func = service.Functions["updateStatus"];
        var param = func.Parameters["newStatus"];

        Assert.IsType<NetRpc.Core.Types.Enum>(param.Type);
        Assert.Equal("Status", ((NetRpc.Core.Types.Enum)param.Type).Identifier);
    }

    [Fact]
    public void Load_EnumAsServiceReturnType_ResolvesCorrectly()
    {
        var document = LoadThrift(@"
            enum Status {
                PENDING = 1,
                ACTIVE = 2
            }
            
            service UserService {
                Status getUserStatus(i32 userId)
            }
        ");

        Assert.Empty(document.Errors);
        var service = document.Services["UserService"];
        var func = service.Functions["getUserStatus"];

        Assert.IsType<NetRpc.Core.Types.Enum>(func.Type);
        Assert.Equal("Status", ((NetRpc.Core.Types.Enum)func.Type).Identifier);
    }

    [Fact]
    public void Load_MultipleEnums_ParsesAllCorrectly()
    {
        var document = LoadThrift(@"
            enum Status {
                PENDING = 1,
                ACTIVE = 2
            }
            
            enum Role {
                ADMIN = 100,
                USER = 101
            }
        ");

        Assert.Empty(document.Errors);
        Assert.Equal(2, document.Enums.Count);
        Assert.True(document.Enums.ContainsKey("Status"));
        Assert.True(document.Enums.ContainsKey("Role"));
    }

    [Fact]
    public void Load_EmptyEnum_ParsesWithNoValues()
    {
        var document = LoadThrift(@"
            enum EmptyStatus {
            }
        ");

        Assert.Empty(document.Errors);
        Assert.Single(document.Enums);
        var emptyStatus = document.Enums["EmptyStatus"];
        Assert.Empty(emptyStatus.Values);
    }

    [Fact]
    public void Load_EnumWithMarkdownInComments_PreservesFormatting()
    {
        var document = LoadThrift(@"
            /**
             * User status enum
             * 
             * ## Usage
             * 
             * - Use PENDING for new users
             * - Use ACTIVE for verified users
             */
            enum Status {
                /**
                 * User is pending approval
                 * 
                 *     Example code:
                 *     status = PENDING
                 */
                PENDING = 1,
                /** User is active */
                ACTIVE = 2
            }
        ");

        Assert.Empty(document.Errors);
        var status = document.Enums["Status"];

        // Check enum description preserves markdown structure
        Assert.NotNull(status.Description);
        Assert.Contains("User status enum", status.Description);
        Assert.Contains("## Usage", status.Description);
        Assert.Contains("- Use PENDING for new users", status.Description);
        Assert.Contains("- Use ACTIVE for verified users", status.Description);

        // Check value description preserves indentation for code blocks
        var pendingDesc = status.Values["PENDING"].Description;
        Assert.NotNull(pendingDesc);
        Assert.Contains("User is pending approval", pendingDesc);
        Assert.Contains("    Example code:", pendingDesc);
        Assert.Contains("    status = PENDING", pendingDesc);

        // Simple single-line comment
        Assert.Contains("User is active", status.Values["ACTIVE"].Description ?? "");
    }

    [Fact]
    public void Load_EnumWithMultilineComment_PreservesLineBreaks()
    {
        var document = LoadThrift(@"
            /**
             * First line
             * Second line
             * Third line
             */
            enum Status {
                PENDING = 1
            }
        ");

        Assert.Empty(document.Errors);
        var status = document.Enums["Status"];
        Assert.NotNull(status.Description);
        Assert.Contains("First line", status.Description);
        Assert.Contains("Second line", status.Description);
        Assert.Contains("Third line", status.Description);
    }

    private Document LoadThrift(string content)
    {
        var loader = new ThriftLoader();
        var document = new Document
        {
            Uri = "test.thrift",
            Content = content
        };
        return loader.Load(document, new DefinitionState());
    }
}