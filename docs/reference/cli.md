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
| `--generator` | `-g`  | Code generator to use. Options: `csharp-jsonapi`, `typescript-client`.                     |
| `--include`   | `-i`  | Add a directory to search for include directives. Can be specified multiple times.         |
| `--output`    | `-o`  | Output file path (optional, defaults to console output).                                   |

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

## lint

Lint Thrift definitions for errors and style issues.

```bash
contractforge lint --entry service.thrift
```
