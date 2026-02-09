using NetRpc.Core.CSharp;
using NetRpc.Core.Thrift;
using NetRpc.Core.Types;

namespace NetRpc.Core.Test.CSharp;

public class CSharpCodeGenEnumTest
{
    [Fact]
    public Task Generate_BasicEnum_ProducesValidCSharpCode()
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

        var state = new DefinitionState();
        state.Documents.Add("test.thrift", document);

        var generator = new CSharpCodeGen();
        var result = generator.Build(state);

        return Verify(result.Output).UseDirectory("Snapshots");
    }

    [Fact]
    public Task Generate_EnumWithNegativeValues_ProducesValidCSharpCode()
    {
        var document = LoadThrift(@"
            namespace cs Test

            enum Priority {
                LOW = -1,
                NORMAL = 0,
                HIGH = 1
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
    public Task Generate_EnumAsFieldType_ProducesValidCSharpCode()
    {
        var document = LoadThrift(@"
            namespace cs Test

            enum Status {
                PENDING = 1,
                ACTIVE = 2
            }

            struct Task {
                1: required string name,
                2: required Status status
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
        return loader.Load(document);
    }
}