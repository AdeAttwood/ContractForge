using DotnetJsonWebApi.Generated;

namespace DotnetJsonWebApi.Service;

public class CalculatorService : ICalculatorService, IRoutedCalculatorService
{
    private readonly ILogger<CalculatorService> _logger;

    public CalculatorService(ILogger<CalculatorService> logger)
    {
        _logger = logger;
    }

    public Task<NumberResult> Add(NumberRequestParams requestParams, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding {A} to {B}", requestParams.A, requestParams.B);
        return Task.FromResult(new NumberResult(requestParams.A + requestParams.B));
    }

    public Task<NumberResult> Subtract(NumberRequestParams requestParams, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Subtracting {A} from {B}", requestParams.A, requestParams.B);

        var result = requestParams.A - requestParams.B;
        if (result == 0)
        {
            return Task.FromResult(new NumberResult(new MessageOnlyError("The service does not handle numbers equal to 0")));
        }

        if (result < 0)
        {
            return Task.FromResult(new NumberResult(new GenericError(501, "The service does not handle numbers less than 0")));
        }

        return Task.FromResult(new NumberResult(result));
    }

    public Task<CalculatorMode> EchoMode(CalculatorMode mode, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(mode);
    }

    public Task<IAsyncEnumerable<long>> Range(NumberRequestParams requestParams, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting range from {A}", System.Text.Json.JsonSerializer.Serialize(requestParams));

        var range = Enumerable.Range((int)requestParams.A, (int)(requestParams.B - requestParams.A + 1))
            .Select(i => (long)i)
            .ToAsyncEnumerable();

        return Task.FromResult(range);
    }

    public Task<int> AddTwoNumbers(int a, int b, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(a + b);
    }

    public Task<long> AddNested(NestedNumberRequestParams requestParams, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(AddNestedNumbers(requestParams));
    }

    public Task<long> AddNestedQuery(NestedNumberRequestParams requestParams, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(AddNestedNumbers(requestParams));
    }

    private static long AddNestedNumbers(NestedNumberRequestParams requestParams)
    {
        return requestParams.Numbers.A + requestParams.Numbers.B;
    }
}
