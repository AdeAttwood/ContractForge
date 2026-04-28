using System.Globalization;

using ContractForge.Core.Types;

namespace ContractForge.Core.Test.Types;

public class ServiceUrlTest
{
    public ServiceUrlTest()
    {
        Inflector.Inflector.SetDefaultCultureFunc = () => new CultureInfo("en-GB");
    }

    [Theory]
    [InlineData(null, "/rpc/calculator-service")]
    [InlineData("rpc", "/rpc/calculator-service")]
    [InlineData("/rpc", "/rpc/calculator-service")]
    [InlineData("rpc/", "/rpc/calculator-service")]
    [InlineData("/rpc/", "/rpc/calculator-service")]
    [InlineData(" api/users ", "/api/users/calculator-service")]
    [InlineData("", "/calculator-service")]
    [InlineData("/", "/calculator-service")]
    public void Url_NormalizesBaseUrl(string? baseUrl, string expected)
    {
        var service = CreateService("Calculator");
        if (baseUrl is not null)
        {
            service.Attributes["http.baseUrl"] = baseUrl;
        }

        Assert.Equal(expected, service.Url());
    }

    private static Service CreateService(string identifier)
    {
        return new Service
        {
            Document = new Document
            {
                Uri = "test.thrift",
                Content = string.Empty
            },
            Point = new Point(),
            Identifier = identifier
        };
    }
}