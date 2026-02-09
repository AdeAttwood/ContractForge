using DotnetJsonWebApi.Generated;

namespace DotnetJsonWebApi.Controllers;

public class CalculatorController : CalculatorBaseController
{
    public CalculatorController(ICalculatorService service): base(service)
    {}
}