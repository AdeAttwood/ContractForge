using ContractForge.Core.CSharp;
using ContractForge.Core.Thrift;
using ContractForge.Core.Types;

namespace ContractForge.Core.Test.CSharp;

public class CSharpCodeGenEnumTest
{
    [Fact]
    public Task Generate_BasicEnum_ProducesValidCSharpCode()
    {
        var document = LoadThrift(@"
            namespace cs Test

            enum Status {
                Pending = 1,
                Active = 2,
                Completed = 3
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
                Low = -1,
                Normal = 0,
                High = 1
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
                Pending = 1,
                Active = 2
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

    [Fact]
    public Task Generate_EnumWithUnderscoreValue_UsesCamelCaseSerializedName()
    {
        var document = LoadThrift(@"
            namespace cs Test

            enum Progress {
                NearlyDone = 1
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