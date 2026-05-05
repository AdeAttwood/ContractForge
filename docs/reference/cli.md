# CLI Reference

The `contractforge` CLI tool generates RPC client and server code from Thrift
IDL files.

## Commands

### generate

Generate code from Thrift definitions (default command).

```bash
contractforge generate [options]
```

Or simply:

```bash
contractforge [options]
```

#### Options

| Option        | Short | Description                                                                                |
| ------------- | ----- | ------------------------------------------------------------------------------------------ |
| `--entry`     | `-e`  | Path to the Thrift IDL file(s). Can be specified multiple times for multiple entry points. |
| `--generator` | `-g`  | Code generator to use. Options: `csharp-jsonapi`, `openapi`, `typescript-client`.          |
| `--include`   | `-i`  | Add a directory to search for include directives. Can be specified multiple times.         |
| `--output`    | `-o`  | Output file path (optional, defaults to console output).                                   |
| `--option`    | `-O`  | Generator option in `key=value` format. Can be specified multiple times.                   |

## Examples

### Single Entry Point

Generate C# code from a single Thrift file:

```bash
contractforge --entry service.thrift --generator csharp-jsonapi --output Service.cs
```

### Multiple Entry Points

Generate code for multiple services that share common types:

```bash
contractforge \
  --entry services/user.thrift \
  --entry services/order.thrift \
  --entry services/payment.thrift \
  --generator csharp-jsonapi \
  --output Services.cs
```

When using multiple entry points, shared includes (e.g., a common types file)
are automatically deduplicated and only generated once.

### With Include Paths

If your Thrift files include other files from different directories:

```bash
contractforge \
  --entry service.thrift \
  --include ./common \
  --include ./shared \
  --generator typescript-client
```

### TypeScript Client Generation

Generate a TypeScript client:

```bash
contractforge --entry api.thrift --generator typescript-client --output api-client.ts
```

### OpenAPI Generation

Generate an OpenAPI JSON document:

```bash
contractforge --entry api.thrift --generator openapi --output openapi.json
```

OpenAPI generator options use OpenAPI-style keys:

```bash
contractforge \
  --entry api.thrift \
  --generator openapi \
  --output openapi.json \
  -O openapi=3.1.0 \
  -O info.title="Users API" \
  -O info.version=1.2.0 \
  -O servers.0.url=https://api.example.com
```

## lint

Lint Thrift definitions for errors and style issues.

```bash
contractforge lint --entry service.thrift
```
