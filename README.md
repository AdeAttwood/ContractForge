# RpcNet

RpcNet is a .NET tool for generating RPC clients and servers from Thrift IDL
files. It supports code generation for C# and TypeScript, enabling seamless
cross-language RPC communication.

## Features

- Parse Thrift IDL files using ANTLR.
- Generate C# server code.
- Generate TypeScript client code.
- Command-line interface for easy integration.

## Installation

1. Clone the repository: `git clone https://github.com/AdeAttwood/RpcNet.git`
1. Build the solution: `dotnet build RpcNet.sln`

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

Options:

- `--entry` (`-e`): Path to the Thrift IDL file.
- `--generator` (`-g`): Code generator (`csharp-jsonapi` for C# or
  `typescript-client` for TypeScript).

Generated code is output to the console. Redirect to a file if needed, e.g.,
`rpcnet ... > output.cs`.

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

## License

MIT License. See LICENSE file for details.
