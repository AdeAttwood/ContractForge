# C# JSON API Generator

The `csharp-jsonapi` generator creates ASP.NET Core server code from Thrift IDL
files, enabling you to build type-safe HTTP APIs with web standards support.

## Overview

This generator produces:

- **DTOs** (Data Transfer Objects) - structs, exceptions, and unions
- **Service interfaces** - for dependency injection
- **Abstract controller classes** - ASP.NET Core MVC controllers
- Support for multiple serialization formats via content negotiation

## Features

- ✅ Abstract controller classes with routing
- ✅ Dependency injection pattern
- ✅ IAsyncEnumerable streaming for list types
- ✅ Multiple serialization formats (JSON, NDJSON)
- ✅ Content negotiation via Accept headers
- ✅ Exception handling with Thrift exceptions
- ✅ Union types (discriminated unions)
- ✅ Documentation comment preservation

## Quick Start

Build your first C# API with RpcNet in 10 minutes. This guide walks you through
creating a simple Calculator service.

### Prerequisites

- .NET 8.0 SDK
- RpcNet CLI installed
- Basic familiarity with ASP.NET Core

### Step 1: Define Your API

Create `calculator.thrift`:

```thrift
namespace csharp Calculator

service Calculator {
  /** Add two numbers */
  i32 add(1: i32 a, 2: i32 b)
  
  /** Subtract two numbers */
  i32 subtract(1: i32 a, 2: i32 b)
}
```

### Step 2: Generate Server Code

```bash
netrpc generate csharp-jsonapi -i calculator.thrift -o Generated/
```

This creates:

- `CalculatorController.cs` - Abstract controller class
- Route: `/api/calculator`
- Methods: `Add(int a, int b)`, `Subtract(int a, int b)`

### Step 3: Create ASP.NET Core Project

```bash
dotnet new webapi -n CalculatorApi
cd CalculatorApi
```

Copy the generated files:

```bash
cp ../Generated/*.cs ./
```

### Step 4: Implement the Service

Create `CalculatorImpl.cs`:

```csharp
using Microsoft.AspNetCore.Mvc;

namespace CalculatorApi;

public class CalculatorImpl : CalculatorController
{
    public override Task<int> Add(int a, int b)
    {
        return Task.FromResult(a + b);
    }

    public override Task<int> Subtract(int a, int b)
    {
        return Task.FromResult(a - b);
    }
}
```

### Step 5: Register and Run

Update `Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();
```

Run the server:

```bash
dotnet run
```

Your API is now running at `http://localhost:5000`!

### Step 6: Test It

Test with curl:

```bash
# Add two numbers
curl -X POST http://localhost:5000/api/calculator/add \
  -H "Content-Type: application/json" \
  -d '{"a": 5, "b": 3}'
# Returns: 8

# Subtract two numbers
curl -X POST http://localhost:5000/api/calculator/subtract \
  -H "Content-Type: application/json" \
  -d '{"a": 10, "b": 4}'
# Returns: 6
```

### What You've Built

- ✅ Type-safe ASP.NET Core API
- ✅ Automatic JSON serialization
- ✅ RESTful HTTP endpoints
- ✅ Ready for production use

### Next Steps

- Add more operations to your service
- Learn about
  [streaming with IAsyncEnumerable](#streaming-with-iasyncenumerable)
- Explore [exception handling](#exception-handling)
- Set up [format negotiation](#format-negotiation) for NDJSON streaming

## Generated Code Structure

### Data Transfer Objects (DTOs)

Thrift structs become C# classes with JSON serialization:

**Thrift:**

```thrift
struct User {
    1: i64 id,
    2: string name,
    3: string? email
}
```

**Generated C#:**

```csharp
public class User
{
    [JsonPropertyName("id")]
    public required long Id { get; set; }
    
    [JsonPropertyName("name")]
    public required string Name { get; set; }
    
    [JsonPropertyName("email")]
    public string? Email { get; set; }
}
```

### Exceptions

Thrift exceptions become C# exception classes:

**Thrift:**

```thrift
exception NotFoundError {
    i32 code,
    string message
}
```

**Generated C#:**

```csharp
public class NotFoundError : Exception
{
    [JsonPropertyName("code")]
    public int Code { get; set; }
    
    public NotFoundError(int code, string message) : base(message)
    {
        Code = code;
    }
}
```

### Union Types

Thrift unions become discriminated unions:

**Thrift:**

```thrift
union Result {
    User success,
    NotFoundError error
}
```

**Generated C#:**

```csharp
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ResultType
{
    [JsonStringEnumMemberName("success")]
    Success,
    [JsonStringEnumMemberName("error")]
    Error,
}

public class Result
{
    [JsonPropertyName("type")]
    public ResultType Type { get; set; }
    
    [JsonPropertyName("success")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public User? Success { get; set; }
    
    public Result(User success)
    {
        Type = ResultType.Success;
        this.Success = success;
    }
    
    [JsonPropertyName("error")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public NotFoundError? Error { get; set; }
    
    public Result(NotFoundError error)
    {
        Type = ResultType.Error;
        this.Error = error;
    }
}
```

### Service Interface

**Thrift:**

```thrift
service UserService {
    User getUser(i64 id)
}
```

**Generated C#:**

```csharp
public interface IUserService
{
    Task<User> GetUser(long id, CancellationToken cancellationToken = default);
}
```

### Abstract Controller

```csharp
[Route("/rpc/user-service")]
public abstract class UserServiceBaseController : Controller
{
    private readonly IUserService _service;
    
    public UserServiceBaseController(IUserService service)
    {
        _service = service;
    }
    
    [HttpPost]
    [Route("get-user")]
    public virtual Task<User> GetUser([FromBody] GetUserRequest request, 
                                      CancellationToken cancellationToken = default)
    {
        return _service.GetUser(request.Id, cancellationToken);
    }
}
```

## Streaming with IAsyncEnumerable

When a Thrift service method returns `list<T>`, the generator creates methods
that return `Task<IAsyncEnumerable<T>>`, enabling efficient streaming of large
datasets.

### Example

**Thrift:**

```thrift
service DataService {
    list<Record> getAllRecords()
}
```

**Generated Interface:**

```csharp
public interface IDataService
{
    Task<IAsyncEnumerable<Record>> GetAllRecords(CancellationToken cancellationToken = default);
}
```

**Implementation:**

```csharp
public class DataService : IDataService
{
    public async Task<IAsyncEnumerable<Record>> GetAllRecords(
        CancellationToken cancellationToken = default)
    {
        // Return an async enumerable
        return GetRecordsAsync(cancellationToken);
    }
    
    private async IAsyncEnumerable<Record> GetRecordsAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var record in _database.StreamRecordsAsync())
        {
            if (cancellationToken.IsCancellationRequested)
                yield break;
                
            yield return record;
        }
    }
}
```

## Format Negotiation

ASP.NET Core supports multiple serialization formats via content negotiation
using the `Accept` HTTP header. This is a web standard (RFC 7231) that allows
clients to specify their preferred response format.

### Supported Formats

| Format     | Accept Header          | Use Case                           |
| ---------- | ---------------------- | ---------------------------------- |
| **JSON**   | `application/json`     | Standard responses, small datasets |
| **NDJSON** | `application/x-ndjson` | Streaming large datasets           |

### Setting Up Format Negotiation

#### 1. Install NDJSON Support

Add the NDJSON package to your project:

```bash
dotnet add package Ndjson.AsyncStreams.AspNetCore.Mvc
```

#### 2. Configure ASP.NET Core

```csharp
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
{
    // Respect the Accept header from client requests
    options.RespectBrowserAcceptHeader = true;
    
    // Return 406 Not Acceptable if client requests unsupported format
    options.ReturnHttpNotAcceptable = true;
})
.AddNdjson(); // Enable NDJSON serialization

var app = builder.Build();
app.MapControllers();
app.Run();
```

### JSON Format (Default)

Returns the entire array at once.

**Request:**

```http
POST /rpc/data-service/get-all-records HTTP/1.1
Host: localhost:5000
Accept: application/json
Content-Type: application/json
```

**Response:**

```http
HTTP/1.1 200 OK
Content-Type: application/json

[
  {"id": 1, "name": "Record 1"},
  {"id": 2, "name": "Record 2"},
  {"id": 3, "name": "Record 3"}
]
```

**Characteristics:**

- ✅ Simple, widely supported
- ✅ Can be parsed as a single JSON array
- ⚠️ Buffers entire response in memory
- ⚠️ Client must wait for all data before processing

### NDJSON Format (Streaming)

Returns newline-delimited JSON objects, streaming one at a time.

**Request:**

```http
POST /rpc/data-service/get-all-records HTTP/1.1
Host: localhost:5000
Accept: application/x-ndjson
Content-Type: application/json
```

**Response:**

```http
HTTP/1.1 200 OK
Content-Type: application/x-ndjson

{"id":1,"name":"Record 1"}
{"id":2,"name":"Record 2"}
{"id":3,"name":"Record 3"}
```

**Characteristics:**

- ✅ Memory efficient (no buffering)
- ✅ Client can process items as they arrive
- ✅ Supports very large datasets
- ✅ Lower time-to-first-byte
- ⚠️ Not a single JSON document

### Client Example

#### Using curl with JSON

```bash
curl -X POST http://localhost:5000/rpc/data-service/get-all-records \
  -H "Accept: application/json" \
  -H "Content-Type: application/json" \
  -d '{}'
```

#### Using curl with NDJSON

```bash
curl -X POST http://localhost:5000/rpc/data-service/get-all-records \
  -H "Accept: application/x-ndjson" \
  -H "Content-Type: application/json" \
  -d '{}'
```

### How ASP.NET Core Handles Format Selection

ASP.NET Core's content negotiation system automatically:

1. **Reads the `Accept` header** from the client request
2. **Matches available formatters** (JSON, NDJSON, etc.)
3. **Selects the best formatter** based on client preferences
4. **Serializes the response** using the selected formatter
5. **Sets the `Content-Type` header** in the response

This happens automatically in the MVC pipeline - your controller and service
code remain format-agnostic.

### Best Practices

**Use JSON when:**

- Returning small datasets (< 1000 items)
- Client needs the entire dataset before processing
- Working with JavaScript clients that expect JSON arrays
- Debugging or testing

**Use NDJSON when:**

- Returning large datasets (> 1000 items)
- Memory efficiency is important
- You want to display results progressively
- Time-to-first-byte matters
- Working with streaming data pipelines

## HTTP Method Routing

By default, all service methods use POST. You can customize this with the
`http.method` attribute:

**Thrift:**

```thrift
service UserService {
    User getUser(i64 id) (http.method = "get")
}
```

When using GET, parameters are bound from query strings instead of the request
body.

## Authorization

You can add ASP.NET Core `[Authorize]` attributes to your services and methods
using the `authorize.policy` attribute. Multiple policies can be specified as a
comma-separated list.

**Thrift:**

```thrift
service Calculator {
  // Apply policy to specific method
  i32 add(1: i32 a, 2: i32 b) (authorize.policy = "AtLeast21")
  
  // Apply multiple policies
  i32 sensitiveOp() (authorize.policy = "AtLeast21, CanEditProducts")
} (authorize.policy = "AuthenticatedUsers") // Apply to all methods in service
```

**Generated C#:**

```csharp
[Authorize(Policy = "AuthenticatedUsers")]
public abstract class CalculatorBaseController : Controller
{
    [Authorize(Policy = "AtLeast21")]
    public virtual Task<int> Add(...)
    
    [Authorize(Policy = "AtLeast21")]
    [Authorize(Policy = "CanEditProducts")]
    public virtual Task<int> SensitiveOp(...)
}
```

## Custom Base URLs

Set a custom base URL for your service:

**Thrift:**

```thrift
service UserService {
    User getUser(i64 id)
} (http.baseUrl = "/api/users")
```

**Generated Route:**

```csharp
[Route("/api/users")]
public abstract class UserServiceBaseController : Controller
```

## Dependency Injection Pattern

The generator creates an interface and expects you to provide the
implementation:

```csharp
// 1. Implement the generated interface
public class UserService : IUserService
{
    private readonly IDbContext _db;
    
    public UserService(IDbContext db)
    {
        _db = db;
    }
    
    public async Task<User> GetUser(long id, CancellationToken cancellationToken)
    {
        return await _db.Users.FindAsync(id, cancellationToken);
    }
}

// 2. Register in ASP.NET Core
builder.Services.AddScoped<IUserService, UserService>();

// 3. Inherit from generated base controller
public class UserController : UserServiceBaseController
{
    public UserController(IUserService service) : base(service)
    {
    }
}
```

This pattern allows you to:

- ✅ Inject dependencies into your service implementation
- ✅ Test your service logic in isolation
- ✅ Override controller behavior when needed
- ✅ Maintain separation of concerns

## Exception Handling

Thrift exceptions are automatically handled by ASP.NET Core's exception
middleware.

**Thrift:**

```thrift
exception ValidationError {
    string message
}

service UserService {
    User createUser(string name) throws (ValidationError err)
}
```

**Implementation:**

```csharp
public Task<User> CreateUser(string name, CancellationToken cancellationToken)
{
    if (string.IsNullOrEmpty(name))
    {
        throw new ValidationError("Name is required");
    }
    
    return Task.FromResult(new User { Name = name });
}
```

The exception will be serialized to JSON and returned to the client with an
appropriate HTTP status code.

## Working Example

See the complete example in `samples/DotnetJsonWebApi/`:

- **Thrift IDL**: `calculator.thrift`
- **Generated code**: `Generated/CalculatorService.cs`
- **Service implementation**: `Service/CalculatorService.cs`
- **Controller**: `Controllers/CalculatorController.cs`
- **ASP.NET Core setup**: `Program.cs`

Run the sample:

```bash
cd samples/DotnetJsonWebApi
dotnet run
```

Test with format negotiation:

```bash
# JSON format
curl http://localhost:5050/rpc/calculator-service/range \
  -H "Accept: application/json" \
  -H "Content-Type: application/json" \
  -d '{"a": 1, "b": 5}'

# NDJSON format (streaming)
curl http://localhost:5050/rpc/calculator-service/range \
  -H "Accept: application/x-ndjson" \
  -H "Content-Type: application/json" \
  -d '{"a": 1, "b": 5}'
```

## Reference

### CLI Options

```bash
netrpc -e <input.thrift> -g csharp-jsonapi -o <output.cs>
```

| Option            | Description                      |
| ----------------- | -------------------------------- |
| `-e, --entry`     | Path to the Thrift IDL file      |
| `-g, --generator` | Generator name: `csharp-jsonapi` |
| `-o, --output`    | Output file path                 |

### Thrift Attributes

| Attribute          | Applies To          | Description                       |
| ------------------ | ------------------- | --------------------------------- |
| `http.method`      | Service functions   | HTTP method (get, post)           |
| `http.baseUrl`     | Services            | Base URL for all routes           |
| `authorize.policy` | Services, Functions | ASP.NET Core Authorization Policy |

### Type Mappings

| Thrift Type | C# Type                                 |
| ----------- | --------------------------------------- |
| `bool`      | `bool`                                  |
| `byte`      | `sbyte`                                 |
| `i8`        | `sbyte`                                 |
| `i16`       | `short`                                 |
| `i32`       | `int`                                   |
| `i64`       | `long`                                  |
| `double`    | `double`                                |
| `string`    | `string`                                |
| `binary`    | `byte[]`                                |
| `uuid`      | `Guid`                                  |
| `list<T>`   | `IAsyncEnumerable<T>` (in return types) |
| `map<K,V>`  | `Dictionary<K, V>`                      |
| `struct`    | `class`                                 |
| `exception` | `class : Exception`                     |
| `union`     | `class` with discriminator              |
| `enum`      | `enum`                                  |

## See Also

- [TypeScript Client Generator](typescript-client.md) - Generate clients for the
  API
- [Thrift IDL Reference](../reference/thrift-idl.md) - Complete type system and
  syntax guide
- [Getting Started Guide](../getting-started/quickstart.md) - Build your first
  RpcNet API
