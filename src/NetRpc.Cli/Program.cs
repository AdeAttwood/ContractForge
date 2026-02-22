// See https://aka.ms/new-console-template for more information

using System.Globalization;

using NetRpc.Cli;

using Spectre.Console.Cli;

CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");

var app = new CommandApp<CodeGenCommand>();

app.Configure(config =>
{
    config.AddCommand<CodeGenCommand>("generate")
        .WithDescription("Generate code from thrift definitions");

    config.AddCommand<LintCommand>("lint")
        .WithDescription("Lint thrift definitions");

    config.AddCommand<LspCommand>("lsp")
        .WithDescription("Start the lsp server over stdio");
});

return app.Run(args);