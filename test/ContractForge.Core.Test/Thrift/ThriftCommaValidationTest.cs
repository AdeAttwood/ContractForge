using ContractForge.Core.Thrift;
using ContractForge.Core.Types;

using Xunit;

namespace ContractForge.Core.Test.Thrift;

public class ThriftCommaValidationTest
{
    [Fact]
    public void Load_StructWithMissingComma_ReportsMissingCommaError()
    {
        var loader = new ThriftLoader();
        var document = new Document
        {
            Uri = "test.thrift",
            Content = """
            struct DateTimeRange {
              required string start
              required string end
            }
            """
        };

        var loadedDoc = loader.Load(document, new DefinitionState());

        Assert.NotEmpty(loadedDoc.Errors);
        Assert.Contains(loadedDoc.Errors, e => e.Message == "Missing comma");
    }

    [Fact]
    public void Load_ServiceWithMissingComma_ReportsMissingCommaError()
    {
        var loader = new ThriftLoader();
        var document = new Document
        {
            Uri = "test.thrift",
            Content = """
            service MyService {
              i32 add(i32 a, i32 b)
              i32 ping()
            }
            """
        };

        var loadedDoc = loader.Load(document, new DefinitionState());

        Assert.NotEmpty(loadedDoc.Errors);
        Assert.Contains(loadedDoc.Errors, e => e.Message == "Missing comma");
    }

    [Fact]
    public void Load_EnumWithMissingComma_ReportsMissingCommaError()
    {
        var loader = new ThriftLoader();
        var document = new Document
        {
            Uri = "test.thrift",
            Content = """
            enum MyEnum {
              ONE = 1
              TWO = 2
            }
            """
        };

        var loadedDoc = loader.Load(document, new DefinitionState());

        Assert.NotEmpty(loadedDoc.Errors);
        Assert.Contains(loadedDoc.Errors, e => e.Message == "Missing comma");
    }
}