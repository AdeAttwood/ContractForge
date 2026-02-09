using NetRpc.Core.Thrift;
using NetRpc.Core.Types;

public class ThriftInvalidThriftTest
{
    private Document LoadInvalidThrift()
    {
        var loader = new ThriftLoader();
        var document = new Document
        {
            Uri = "test.thrift",
            Content = """
            namespace cs Test.ThriftListenerTest

            service MyService {
              i32 add(i32 a, i32 b)
              i32 ping()
            }
            """
        };

        return loader.Load(document);
    }

    [Fact]
    public void Load_WithInvalidThrift_HasErrors()
    {
        var document = LoadInvalidThrift();
        Assert.NotEmpty(document.Errors);
    }

    private Document LoadWithDuplicateNamespace()
    {
        var loader = new ThriftLoader();
        var document = new Document
        {
            Uri = "test.thrift",
            Content = """
            namespace cs Test.ThriftListenerTest

            namespace cs Another.Test
            """
        };

        return loader.Load(document);
    }

    [Fact]
    public void Load_WithDuplicateNamespace_HasErrors()
    {
        var document = LoadWithDuplicateNamespace();
        Assert.NotEmpty(document.Errors);
    }
}