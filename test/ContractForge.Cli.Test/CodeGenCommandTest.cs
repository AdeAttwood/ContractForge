using Spectre.Console.Cli.Testing;

namespace ContractForge.Cli.Test;

public class CodeGenCommandTest : IDisposable
{
    private readonly string _tempDirectory;

    public CodeGenCommandTest()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), $"contractforge-cli-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDirectory);
    }

    [Fact]
    public void Generate_WithInvalidGenerator_ShowsAvailableGeneratorsAndSuggestion()
    {
        var entryPoint = WriteThriftFile("service.thrift", "service UserService {}\n");
        var app = CreateApp();

        var result = app.Run("generate", "--entry", entryPoint, "--generator", "typescript-clien");

        Assert.Equal(1, result.ExitCode);
        Assert.Contains("Invalid generator 'typescript-clien'.", result.Output);
        Assert.Contains("Available generators: csharp-jsonapi, openapi, typescript-client", result.Output);
        Assert.Contains("Did you mean 'typescript-client'?", result.Output);
    }

    [Fact]
    public void Generate_DefaultCommand_AllowsRootInvocation()
    {
        var entryPoint = WriteThriftFile("service.thrift", "service UserService {}\n");
        var app = CreateApp();

        var result = app.Run("--entry", entryPoint, "--generator", "typescript-clien");

        Assert.Equal(1, result.ExitCode);
        Assert.Contains("Invalid generator 'typescript-clien'.", result.Output);
    }

    [Fact]
    public void Generate_WithMissingGenerator_ShowsAvailableGenerators()
    {
        var entryPoint = WriteThriftFile("service.thrift", "service UserService {}\n");
        var app = CreateApp();

        var result = app.Run("generate", "--entry", entryPoint);

        Assert.Equal(1, result.ExitCode);
        Assert.Contains("A generator is required.", result.Output);
        Assert.Contains("Available generators: csharp-jsonapi, openapi, typescript-client", result.Output);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
    }

    private CommandAppTester CreateApp()
    {
        CliAppFactory.ConfigureCulture();

        var app = new CommandAppTester();
        app.SetDefaultCommand<CodeGenCommand>();
        app.Configure(CliAppFactory.Configure);
        return app;
    }

    private string WriteThriftFile(string fileName, string content)
    {
        var filePath = Path.Combine(_tempDirectory, fileName);
        File.WriteAllText(filePath, content);
        return filePath;
    }
}