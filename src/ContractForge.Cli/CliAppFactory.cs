using System.Globalization;

using Spectre.Console.Cli;

namespace ContractForge.Cli;

public static class CliAppFactory
{
    public static void ConfigureCulture()
    {
        CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
    }

    public static void Configure(IConfigurator config)
    {
        config.SetApplicationName("contractforge");
        config.UseAssemblyInformationalVersion();

        config.AddCommand<CodeGenCommand>("generate")
            .WithDescription("Generate code from thrift definitions");

        config.AddCommand<LintCommand>("lint")
            .WithDescription("Lint thrift definitions");

        config.AddCommand<LspCommand>("lsp")
            .WithDescription("Start the lsp server over stdio");
    }

    public static CommandApp<CodeGenCommand> Create()
    {
        ConfigureCulture();

        var app = new CommandApp<CodeGenCommand>();
        app.Configure(Configure);
        return app;
    }
}