# ContractForge Documentation

**Modern RPC for .NET with Web Standards**

ContractForge is a .NET tool that generates RPC clients and servers from Thrift IDL
files, enabling type-safe cross-language communication using standard HTTP,
JSON, and modern web APIs.

## Key Features

- **Web Standards Compatible** - Uses HTTP, fetch API, and content negotiation
- **Framework Integration** - Deep ASP.NET Core integration with dependency
  injection
- **Protocol Agnostic** - Support for JSON and NDJSON streaming
- **Type Safe** - Full type safety across C# and TypeScript
- **Streaming Support** - Efficient data streaming with NDJSON
- **Multi-Platform** - Generate servers and clients for different platforms

## Quick Start

```bash
# Install the CLI tool
dotnet tool install -g ContractForge.Cli

# Generate a server from Thrift IDL
contractforge generate csharp-jsonapi -i service.thrift -o Generated/

# Generate a TypeScript client
contractforge generate typescript-client -i service.thrift -o client.ts
```

## Who Is This For?

- **Backend .NET Developers** - Build type-safe APIs with ASP.NET Core
- **Frontend Developers** - Get type-safe clients that work everywhere (Node,
  Deno, Bun, browsers)
- **API Architects** - Design APIs with IDL-first approach
- **Teams** - Share type definitions across microservices

## Why ContractForge?

Unlike gRPC (HTTP/2 + Protobuf), ContractForge uses standard HTTP/1.1 with JSON, making
it:

- ✅ **Browser-friendly** - Works with standard fetch API, no special libraries
- ✅ **Debuggable** - Use browser DevTools, Postman, curl
- ✅ **Tool-friendly** - Standard HTTP and JSON for easy integration
- ✅ **Flexible** - Support streaming with NDJSON over HTTP

Unlike hand-written REST APIs, ContractForge gives you:

- ✅ **Type safety** - Compile-time checks across client and server
- ✅ **Code generation** - No manual DTO writing or API client code
- ✅ **IDL-first** - API contract as code, version controlled
- ✅ **Consistency** - Same patterns across all services

## Example

**Define your API in Thrift IDL:**

```thrift
struct User {
  1: i32 id
  2: string name
  3: string email
}

exception UserNotFound {
  1: string message
}

service UserService {
  User getUser(1: i32 id) throws (1: UserNotFound notFound)
  list<User> listUsers()
}
```

**Generated C# Controller:**

```cs
[ApiController]
[Route("api/user-service")]
public abstract class UserServiceController : ControllerBase
{
    [HttpGet("get-user")]
    public abstract Task<User> GetUser(int id);
    
    [HttpGet("list-users")]
    public abstract IAsyncEnumerable<User> ListUsers();
}
```

**Generated TypeScript Client:**

```typescript
class UserServiceClient {
  async getUser(id: number): Promise<User> {/* ... */}
  async listUsers(): Promise<Array<User>> {/* ... */}
  async *listUsersStream(): AsyncIterable<User> {/* ... */}
}
```

## Community & Support

- **GitHub**: [AdeAttwood/ContractForge](https://github.com/AdeAttwood/ContractForge)
- **Issues**:
  [Report bugs or request features](https://github.com/AdeAttwood/ContractForge/issues)
- **Discussions**:
  [Ask questions and share ideas](https://github.com/AdeAttwood/ContractForge/discussions)
