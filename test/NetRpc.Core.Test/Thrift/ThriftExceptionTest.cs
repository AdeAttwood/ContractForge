using NetRpc.Core;
using NetRpc.Core.Thrift;
using NetRpc.Core.Types;

public class ThriftExceptionTest
{
    [Fact]
    public void Load_WithException_HasNoErrors()
    {
        var loader = new ThriftLoader();
        var document = new Document
        {
            Uri = "test.thrift",
            Content = """
            namespace cs Test.ThriftExceptionTest

            exception MyException {
              1: string message
            }
            """
        };

        var result = loader.Load(document, new DefinitionState());
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Load_WithException_HasOneException()
    {
        var loader = new ThriftLoader();
        var document = new Document
        {
            Uri = "test.thrift",
            Content = """
            namespace cs Test.ThriftExceptionTest

            exception MyException {
              1: string message
            }
            """
        };

        var result = loader.Load(document, new DefinitionState());
        Assert.Single(result.Exceptions);
    }

    [Fact]
    public void Load_WithException_HasCorrectExceptionFields()
    {
        var loader = new ThriftLoader();
        var document = new Document
        {
            Uri = "test.thrift",
            Content = """
            namespace cs Test.ThriftExceptionTest

            exception MyException {
              1: string message,
              2: i32 code
            }
            """
        };

        var result = loader.Load(document, new DefinitionState());
        var exception = result.Exceptions["MyException"];
        // Assert.Equal(2, exception.Fields.Count);
        Assert.Contains("message", exception.Fields.Keys);
        Assert.Contains("code", exception.Fields.Keys);
    }

    [Fact]
    public void Load_WithFunctionThrows_HasNoErrors()
    {
        var loader = new ThriftLoader();
        var document = new Document
        {
            Uri = "test.thrift",
            Content = """
            namespace cs Test.ThriftExceptionTest

            exception MyException {
              1: string message;
            }

            service MyService {
              i32 add(i32 a, i32 b) throws (1: MyException ex);
            }
            """
        };

        var result = loader.Load(document, new DefinitionState());
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Load_WithFunctionThrows_HasCorrectExceptionInFunction()
    {
        var loader = new ThriftLoader();
        var document = new Document
        {
            Uri = "test.thrift",
            Content = """
            namespace cs Test.ThriftExceptionTest

            exception MyException {
              1: string message;
            }

            service MyService {
              i32 add(i32 a, i32 b) throws (1: MyException ex);
            }
            """
        };

        var result = loader.Load(document, new DefinitionState());
        var service = result.Services["MyService"];
        var function = service.Functions["add"];
        Assert.Single(function.Exceptions);
        Assert.Contains("ex", function.Exceptions.Keys);
    }
}