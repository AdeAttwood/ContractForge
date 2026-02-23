using ContractForge.Core;
using ContractForge.Core.Thrift;
using ContractForge.Core.Types;

namespace ContractForge.Core.Test.Thrift;

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

    [Fact]
    public void Load_MultipleEntryPoints_WithSharedInclude_DeduplicatesDocuments()
    {
        // common.thrift - shared types
        var commonPath = Path.Combine(_tempDir, "common.thrift");
        File.WriteAllText(commonPath, @"
            namespace cs Common
            struct SharedType {
                1: i32 id,
                2: string name
            }
        ");

        // service1.thrift
        var service1Path = Path.Combine(_tempDir, "service1.thrift");
        File.WriteAllText(service1Path, @"
            namespace cs Service1
            include ""common.thrift""

            service UserService {
                common.SharedType getUser(1: i32 id)
            }
        ");

        // service2.thrift
        var service2Path = Path.Combine(_tempDir, "service2.thrift");
        File.WriteAllText(service2Path, @"
            namespace cs Service2
            include ""common.thrift""

            service OrderService {
                common.SharedType getOrder(1: i32 id)
            }
        ");

        var state = new DefinitionState();
        state.AddIncludePath(_tempDir);

        // Load both entry points
        var doc1 = state.Load(service1Path);
        var doc2 = state.Load(service2Path);

        // Both documents should have no errors
        Assert.Empty(doc1.Errors);
        Assert.Empty(doc2.Errors);

        // Verify both services are loaded
        Assert.True(doc1.Services.ContainsKey("UserService"));
        Assert.True(doc2.Services.ContainsKey("OrderService"));

        // Verify shared type is resolved correctly in both
        var userService = doc1.Services["UserService"];
        var getUserFunc = userService.Functions["getUser"];
        Assert.IsType<Struct>(getUserFunc.Type);
        Assert.Equal("SharedType", ((Struct)getUserFunc.Type).Identifier);

        var orderService = doc2.Services["OrderService"];
        var getOrderFunc = orderService.Functions["getOrder"];
        Assert.IsType<Struct>(getOrderFunc.Type);
        Assert.Equal("SharedType", ((Struct)getOrderFunc.Type).Identifier);

        // Verify deduplication: common.thrift should only be loaded once
        // The state should have: service1, service2, and common (3 documents total)
        Assert.Equal(3, state.Documents.Count);
        Assert.Single(state.Documents.Values, d => d.Uri == commonPath);
    }
}