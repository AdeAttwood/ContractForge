using ContractForge.Core;
using ContractForge.Core.Thrift;
using ContractForge.Core.Types;

public class ThriftBasicServiceLoadTest
{
    private Document Load()
    {
        var loader = new ThriftLoader();
        var document = new Document
        {
            Uri = "test.thrift",
            Content = """
            namespace cs Test.ThriftListenerTest

            service MyService {
              i32 add(i32 a, i32 b)
            }
            """
        };

        return loader.Load(document, new DefinitionState());
    }

    [Fact]
    public void Load_WithBasicService_HasNoErrors()
    {
        var document = Load();
        Assert.Empty(document.Errors);
    }

    [Fact]
    public void Load_WithBasicService_HasTheCorrectNamespace()
    {
        var document = Load();
        Assert.Equal("Test.ThriftListenerTest", document.Namespaces["cs"]);
    }

    [Fact]
    public void Load_WithBasicService_HasOneService()
    {
        var document = Load();
        Assert.Single(document.Services);
    }

    [Fact]
    public void Load_WithBasicService_HasOneFunction()
    {
        var document = Load();
        Assert.Single(document.Services.Values.First().Functions);
    }

    private Document LoadServiceWithNoFunctions()
    {
        var loader = new ThriftLoader();
        var document = new Document
        {
            Uri = "test.thrift",
            Content = """
            namespace cs Test.ThriftListenerTest

            service EmptyService {
            }
            """
        };

        return loader.Load(document, new DefinitionState());
    }

    [Fact]
    public void Load_WithServiceWithNoFunctions_HasNoErrors()
    {
        var document = LoadServiceWithNoFunctions();
        Assert.Empty(document.Errors);
    }

    [Fact]
    public void Load_WithServiceWithNoFunctions_HasOneService()
    {
        var document = LoadServiceWithNoFunctions();
        Assert.Single(document.Services);
    }

    [Fact]
    public void Load_WithServiceWithNoFunctions_HasNoFunctions()
    {
        var document = LoadServiceWithNoFunctions();
        Assert.Empty(document.Services.Values.First().Functions);
    }
}