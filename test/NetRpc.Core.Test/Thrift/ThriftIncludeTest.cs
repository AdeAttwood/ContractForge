using NetRpc.Core;
using NetRpc.Core.Thrift;
using NetRpc.Core.Types;

namespace NetRpc.Core.Test.Thrift;

public class ThriftIncludeTest : IDisposable
{
    private readonly string _tempDir;

    public ThriftIncludeTest()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
    }

    [Fact]
    public void Load_IncludeFile_ResolvesTypeFromIncludedFile()
    {
        // shared.thrift
        var sharedPath = Path.Combine(_tempDir, "shared.thrift");
        File.WriteAllText(sharedPath, @"
            namespace cs Shared
            struct User {
                1: i32 id,
                2: string name
            }
        ");

        // main.thrift
        var mainPath = Path.Combine(_tempDir, "main.thrift");
        File.WriteAllText(mainPath, @"
            namespace cs Main
            include ""shared.thrift""

            struct Group {
                1: i32 id,
                2: shared.User owner
            }
        ");

        var state = new DefinitionState();
        // Note: We don't strictly need AddIncludePath here because they are in the same dir
        // but it doesn't hurt.
        var document = state.Load(mainPath);

        Assert.Empty(document.Errors);

        // Verify struct in main
        Assert.True(document.Structs.ContainsKey("Group"));
        var group = document.Structs["Group"];

        // Verify field type
        var ownerField = group.Fields["owner"];
        Assert.IsType<Struct>(ownerField.Type);
        var userStruct = (Struct)ownerField.Type;
        Assert.Equal("User", userStruct.Identifier);

        // Verify included document
        Assert.True(document.IncludedDocuments.ContainsKey("shared"));
    }

    [Fact]
    public void Load_IncludeFileWithAlias_ResolvesTypeFromAlias()
    {
        // shared.thrift
        var sharedPath = Path.Combine(_tempDir, "shared_types.thrift");
        File.WriteAllText(sharedPath, @"
            namespace cs Shared
            struct User {
                1: i32 id
            }
        ");

        // main.thrift
        var mainPath = Path.Combine(_tempDir, "main_alias.thrift");
        File.WriteAllText(mainPath, @"
            namespace cs Main
            include ""shared_types.thrift"" as types

            struct Group {
                1: i32 id,
                2: types.User owner
            }
        ");

        var state = new DefinitionState();
        var document = state.Load(mainPath);

        Assert.Empty(document.Errors);

        // Verify struct in main
        var group = document.Structs["Group"];
        var ownerField = group.Fields["owner"];
        Assert.IsType<Struct>(ownerField.Type);
    }

    [Fact]
    public void Load_IncludeFileFromIncludePath_ResolvesCorrectly()
    {
        // Create a subdirectory for includes
        var includeDir = Path.Combine(_tempDir, "includes");
        Directory.CreateDirectory(includeDir);

        // shared.thrift in includes dir
        var sharedPath = Path.Combine(includeDir, "common.thrift");
        File.WriteAllText(sharedPath, @"
            namespace cs Shared
            struct Status {
                1: i32 code
            }
        ");

        // main.thrift in root temp dir
        var mainPath = Path.Combine(_tempDir, "main_include_path.thrift");
        File.WriteAllText(mainPath, @"
            namespace cs Main
            include ""common.thrift""

            struct Response {
                1: common.Status status
            }
        ");

        var state = new DefinitionState();
        state.AddIncludePath(includeDir); // Add includes dir to path
        var document = state.Load(mainPath);

        Assert.Empty(document.Errors);

        // Verify struct in main
        var response = document.Structs["Response"];
        var statusField = response.Fields["status"];
        Assert.IsType<Struct>(statusField.Type);
    }
}