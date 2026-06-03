using System.Reflection;

using Spectre.Console.Cli.Testing;

namespace ContractForge.Cli.Test;

public class CliAppFactoryTest
{
    [Theory]
    [InlineData("--version")]
    [InlineData("-v")]
    public void VersionOption_PrintsApplicationVersion(string versionOption)
    {
        var app = CreateApp();
        var expectedVersion = typeof(CliAppFactory).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;

        var result = app.Run(versionOption);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(expectedVersion, result.Output.Trim());
    }

    private static CommandAppTester CreateApp()
    {
        CliAppFactory.ConfigureCulture();

        var app = new CommandAppTester();
        app.SetDefaultCommand<CodeGenCommand>();
        app.Configure(CliAppFactory.Configure);
        return app;
    }
}