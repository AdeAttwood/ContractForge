// See https://aka.ms/new-console-template for more information

using System.Globalization;

using NetRpc.Cli;

using Spectre.Console.Cli;

CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");

var app = new CommandApp<CodeGenCommand>();
return app.Run(args);