# Quickstart

Get started with ContractForge in 5 minutes. This guide shows you how to define
an API and generate code for your platform.

## What You'll Learn

- How to define APIs with Thrift IDL
- How to generate code with the ContractForge CLI
- Where to go next for your use case

## Prerequisites

- [ContractForge CLI installed](installation.md)
- Your target runtime (.NET 8.0+, Node.js 18+, Deno, or Bun)

## Step 1: Define Your API

Create a Thrift IDL file that describes your service. This example shows a
simple Calculator service:

**Create `calculator.thrift`:**

```thrift
namespace csharp Calculator
namespace ts calculator

/**
 * Calculator service with basic arithmetic operations
 */
service Calculator {
  /** Add two numbers */
  i32 add(1: i32 a, 2: i32 b)
  
  /** Subtract two numbers */
  i32 subtract(1: i32 a, 2: i32 b)
  
  /** Multiply two numbers */
  i32 multiply(1: i32 a, 2: i32 b)
  
  /** Divide two numbers */
  double divide(1: i32 a, 2: i32 b)
}
```

This defines a service with four operations. The Thrift IDL is language-agnostic
and serves as the contract between your client and server.

## Step 2: Generate Code

ContractForge provides generators for different platforms. Choose based on what
you're building:

### Server Generators

Generate server-side code for your backend:

#### C# ASP.NET Core

```bash
contractforge --entry calculator.thrift --generator csharp-jsonapi --output Server/Generated/
```

**What you get:**

- Abstract controller classes with routing
- Service interfaces for dependency injection
- DTO models with JSON serialization
- Support for streaming with `IAsyncEnumerable<T>`

**Next steps:**
[Complete C# Server Guide →](../generators/csharp-jsonapi.md#quick-start)

---

### Client Generators

Generate client-side code for your frontend or service-to-service calls:

#### TypeScript (Node/Deno/Bun/Browser)

```bash
contractforge --entry calculator.thrift --generator typescript-client --output client.ts
```

**What you get:**

- Type-safe interfaces and types
- Fetch-based HTTP client
- Streaming support with `AsyncIterable<T>`
- Works in Node.js, Deno, Bun, and browsers

**Next steps:**
[Complete TypeScript Client Guide →](../generators/typescript-client.md#quick-start)

---

### Specification Generators

Generate an OpenAPI document for Swagger UI, documentation, or client tooling:

#### OpenAPI JSON

```bash
contractforge --entry calculator.thrift --generator openapi --output openapi.json
```

**What you get:**

- OpenAPI 3.0.3 JSON by default
- Schemas for Thrift structs, exceptions, enums, and unions
- Paths matching the generated JSON API routes
- JSON and NDJSON response media types for list returns

**Next steps:** [Complete OpenAPI Guide →](../generators/openapi.md#quick-start)

---

## Step 3: Follow Your Generator's Guide

Each generator has detailed documentation covering:

- ✅ Implementation examples
- ✅ Framework integration
- ✅ Running and testing
- ✅ Best practices
- ✅ Troubleshooting

Click the links above to continue with your chosen generator.

## Common Patterns

### Full-Stack Project

Most projects use multiple generators together:

```bash
# Backend API
contractforge --entry calculator.thrift --generator csharp-jsonapi --output Server/Generated/

# Frontend client
contractforge --entry calculator.thrift --generator typescript-client --output client/api.ts

# API specification
contractforge --entry calculator.thrift --generator openapi --output docs/openapi.json
```

Both share the same Thrift IDL, ensuring type consistency across your stack.

### Multiple Services (Microservices)

Generate multiple services from different IDL files:

```bash
# User service
contractforge --entry user-service.thrift --generator csharp-jsonapi --output UserService/Generated/

# Order service
contractforge --entry order-service.thrift --generator csharp-jsonapi --output OrderService/Generated/

# Product service
contractforge --entry product-service.thrift --generator csharp-jsonapi --output ProductService/Generated/
```

Each service can be developed, deployed, and scaled independently.

## What You've Learned

- ✅ How to define APIs with Thrift IDL
- ✅ How to generate code with the ContractForge CLI
- ✅ Available generators for different platforms
- ✅ Common project patterns

## Next Steps

Ready to build? Continue with your chosen generator:

- **[C# Server Guide](../generators/csharp-jsonapi.md)** - Build ASP.NET Core
  APIs
- **[TypeScript Client Guide](../generators/typescript-client.md)** - Build
  type-safe clients
- **[OpenAPI Guide](../generators/openapi.md)** - Generate API specifications
- **[Thrift IDL Reference](../reference/thrift-idl.md)** - Learn the complete
  type system
- **[CLI Reference](../reference/cli.md)** - All CLI commands and options

## Need Help?

- **GitHub Issues**:
  [Report bugs or request features](https://github.com/AdeAttwood/ContractForge/issues)
- **GitHub Discussions**:
  [Ask questions and share ideas](https://github.com/AdeAttwood/ContractForge/discussions)
