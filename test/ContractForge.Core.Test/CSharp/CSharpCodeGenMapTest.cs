using ContractForge.Core.CSharp;
using ContractForge.Core.Thrift;
using ContractForge.Core.Types;

namespace ContractForge.Core.Test.CSharp;

public class CSharpCodeGenMapTest
{
    [Fact]
    public Task Generate_BasicMap_ProducesValidCSharpCode()
    {
        var document = LoadThrift(@"
            namespace cs Test

            struct UserPreferences {
                required map<string, string> settings
            }
        ");

        Assert.Empty(document.Errors);

        var state = new DefinitionState();
        state.Documents.Add("test.thrift", document);

        var generator = new CSharpCodeGen();
        var result = generator.Build(state);

        return Verify(result.Output).UseDirectory("Snapshots");
    }

    [Fact]
    public Task Generate_MapWithComplexValueType_ProducesValidCSharpCode()
    {
        var document = LoadThrift(@"
            namespace cs Test

            struct User {
                required i32 id,
                required string name
            }

            struct UserDirectory {
                required map<i32, User> users
            }
        ");

        Assert.Empty(document.Errors);

        var state = new DefinitionState();
        state.Documents.Add("test.thrift", document);

        var generator = new CSharpCodeGen();
        var result = generator.Build(state);

        return Verify(result.Output).UseDirectory("Snapshots");
    }

    [Fact]
    public Task Generate_ServiceWithMapReturnType_ProducesValidCSharpCode()
    {
        var document = LoadThrift(@"
            namespace cs Test

            struct User {
                required i32 id,
                required string name
            }

            service UserService {
                map<i32, User> getUserMap(list<i32> ids)
            }
        ");

        Assert.Empty(document.Errors);

        var state = new DefinitionState();
        state.Documents.Add("test.thrift", document);

        var generator = new CSharpCodeGen();
        var result = generator.Build(state);

        return Verify(result.Output).UseDirectory("Snapshots");
    }

    [Fact]
    public Task Generate_MapWithListValue_ProducesValidCSharpCode()
    {
        var document = LoadThrift(@"
            namespace cs Test

            struct ComplexData {
                required map<string, list<i32>> dataMap
            }
        ");

        Assert.Empty(document.Errors);

        var state = new DefinitionState();
        state.Documents.Add("test.thrift", document);

        var generator = new CSharpCodeGen();
        var result = generator.Build(state);

        return Verify(result.Output).UseDirectory("Snapshots");
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