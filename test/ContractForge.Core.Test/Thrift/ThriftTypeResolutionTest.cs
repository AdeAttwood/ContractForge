using ContractForge.Core.Thrift;
using ContractForge.Core.Types;

using Xunit;

namespace ContractForge.Core.Test.Thrift;

public class ThriftTypeResolutionTest
{
    [Fact]
    public void Load_WithForwardReference_ResolvesCorrectly()
    {
        var loader = new ThriftLoader();
        var document = new Document
        {
            Uri = "test.thrift",
            Content = """
            struct A {
                1: required B b
            }

            struct B {
                1: required i32 val
            }
            """
        };

        var loadedDoc = loader.Load(document, new DefinitionState());

        Assert.Empty(loadedDoc.Errors);

        var structA = loadedDoc.Structs["A"];
        var fieldB = structA.Fields["b"];
        Assert.IsType<Struct>(fieldB.Type);
        Assert.Equal("B", ((Struct)fieldB.Type).Identifier);
    }

    [Fact]
    public void Load_WithMissingType_ReportsError()
    {
        var loader = new ThriftLoader();
        var document = new Document
        {
            Uri = "test.thrift",
            Content = """
            struct A {
                1: required MissingType b
            }
            """
        };

        var loadedDoc = loader.Load(document, new DefinitionState());

        Assert.NotEmpty(loadedDoc.Errors);
        Assert.Contains(loadedDoc.Errors, e => e.Id == "NR0002");
    }
}