using NetRpc.Core.Thrift;
using NetRpc.Core.Types;

public class ThriftMultipleFunctionsTest
{
    private Document LoadWithMultipleFunctions()
    {
        var loader = new ThriftLoader();
        var document = new Document
        {
            Uri = "test.thrift",
            Content = """
            namespace cs Test.ThriftListenerTest

            service MyService {
              i32 add(i32 a, i32 b),
              i32 ping(),
              i32 greet(i32 name)
            }
            """
        };

        return loader.Load(document);
    }

    [Fact]
    public void Load_WithMultipleFunctions_HasNoErrors()
    {
        var document = LoadWithMultipleFunctions();
        Assert.Empty(document.Errors);
    }

    [Fact]
    public void Load_WithMultipleFunctions_HasCorrectFunctionCount()
    {
        var document = LoadWithMultipleFunctions();
        Assert.Equal(3, document.Services.Values.First().Functions.Count);
    }
}