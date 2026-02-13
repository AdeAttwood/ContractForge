# NetRpc

NetRpc is a .NET tool for generating RPC clients and servers from Thrift IDL
files. It supports code generation for C# and TypeScript, enabling seamless
cross-language RPC communication.

## Features

- Parse Thrift IDL files using ANTLR.
- Generate C# server code.
- Generate TypeScript client code.
- Command-line interface for easy integration.

## Installation

1. Clone the repository: `git clone https://github.com/AdeAttwood/NetRpc.git`
1. Build the solution: `dotnet build NetRpc.sln`

## Usage

### Installing the Tool

1. Build and pack the tool: `dotnet pack src/NetRpc.Cli/NetRpc.Cli.csproj`
2. Install globally:
   `dotnet tool install -g --add-source ./src/NetRpc.Cli/bin/Release rpcnet.cli`

### CLI Tool

Run the installed tool to generate code from a Thrift file:

```bash
rpcnet --entry path/to/thrift/file.thrift --generator csharp-jsonapi
```

You can also specify multiple entry points for services that share common
includes:

```bash
rpcnet --entry service1.thrift --entry service2.thrift --generator csharp-jsonapi
```

Options:

- `--entry` (`-e`): Path to the Thrift IDL file. Can be specified multiple
  times for multiple entry points.
- `--generator` (`-g`): Code generator (`csharp-jsonapi` for C# or
  `typescript-client` for TypeScript).
- `--include` (`-i`): Add a directory to the list of directories searched for
  include directives. Can be specified multiple times.
- `--output` (`-o`): The output file to save the generated code to (optional,
  defaults to console).

Generated code is output to the console by default. Redirect to a file if
needed, e.g., `rpcnet ... > output.cs`, or use the `--output` option.

## Examples

See the `samples/` directory for working examples:

- `DotnetJsonWebApi`: ASP.NET Core Web API with generated C# service.
- `DenoTypescriptClient`: Deno TypeScript client for RPC calls.

## Building from Source

1. Restore dependencies: `dotnet restore`
2. Build: `dotnet build`
3. Run tests: `dotnet test`

## Contributing

Contributions are welcome! Please submit issues and pull requests.
