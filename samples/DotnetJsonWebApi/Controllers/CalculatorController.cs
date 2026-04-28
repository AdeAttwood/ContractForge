using DotnetJsonWebApi.Generated;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DotnetJsonWebApi.Controllers;

public class CalculatorController : CalculatorBaseController
{
    private const string ClientTokenHeader = "X-Client-Token";
    private const string ClientTokenValue = "sample-token";

    public CalculatorController(ICalculatorService service): base(service)
    {}

    [HttpGet]
    [Route("add-two-numbers")]
    public override Task<int> AddTwoNumbers([FromQuery] int a, [FromQuery] int b, CancellationToken cancellationToken = default)
    {
        if (!Request.Headers.TryGetValue(ClientTokenHeader, out var token) || token != ClientTokenValue)
        {
            Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.FromResult(0);
        }

        return base.AddTwoNumbers(a, b, cancellationToken);
    }
}

public class RoutedCalculatorController : RoutedCalculatorBaseController
{
    public RoutedCalculatorController(IRoutedCalculatorService service): base(service)
    {}
}
