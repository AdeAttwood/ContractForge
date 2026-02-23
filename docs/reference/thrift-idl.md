# Thrift IDL Reference

ContractForge uses Thrift Interface Definition Language (IDL) to define RPC
services, data types, and API contracts. This reference covers all supported
features and syntax for defining type-safe, cross-language APIs.

## Overview

ContractForge's Thrift IDL implementation focuses on single-file service
definitions, making it ideal for building modern RPC APIs without the complexity
of multi-file type systems. All definitions—types, services, and
exceptions—coexist in a single `.thrift` file.

**Key Characteristics:**

- Single-file architecture for simplicity
- Full support for structs, unions, enums, and exceptions
- Rich type system with primitives, lists, and maps
- Service attributes for HTTP routing customization
- Markdown-enabled documentation comments

## Type System

### Primitive Types

ContractForge supports 10 primitive types that map to native types in C# and
TypeScript:

| Thrift Type | Description           | C# Type  | TypeScript Type |
| ----------- | --------------------- | -------- | --------------- |
| `bool`      | Boolean value         | `bool`   | `boolean`       |
| `byte`      | 8-bit signed integer  | `sbyte`  | `number`        |
| `i8`        | 8-bit signed integer  | `sbyte`  | `number`        |
| `i16`       | 16-bit signed integer | `short`  | `number`        |
| `i32`       | 32-bit signed integer | `int`    | `number`        |
| `i64`       | 64-bit signed integer | `long`   | `number`        |
| `double`    | 64-bit float          | `double` | `number`        |
| `string`    | UTF-8 text            | `string` | `string`        |
| `binary`    | Byte array            | `byte[]` | `Uint8Array`    |
| `uuid`      | UUID/GUID             | `Guid`   | `string`        |

**Example:**

```thrift
struct UserProfile {
  i64 userId,
  string username,
  bool isActive,
  double rating,
  uuid sessionId
}
```

### Container Types

#### Lists

Generic sequences of elements:

```thrift
list<i32>              // List of integers
list<string>           // List of strings
list<User>             // List of custom types
list<list<i32>>        // Nested lists
```

**Type Mappings:**

- **C#**: `IAsyncEnumerable<T>` (for streaming in return types)
- **TypeScript**: `Array<T>` or `AsyncIterable<T>` (streaming)

**Example:**

```thrift
service UserService {
  list<User> getAllUsers()          // Returns all users
  list<i64> getUserIds(i32 limit)   // Returns user IDs
}
```

#### Maps

Key-value pairs with generic types:

```thrift
map<string, i32>           // String to integer mapping
map<i64, User>             // ID to User mapping
map<string, list<i32>>     // Nested containers
```

**Type Mappings:**

- **C#**: `Dictionary<TKey, TValue>`
- **TypeScript**: `Record<K, V>`

**Example:**

```thrift
struct CacheEntry {
  map<string, string> metadata,
  map<i32, list<string>> tags
}
```

### Custom Types

#### Structs

Data transfer objects with typed fields:

```thrift
struct Address {
  string street,
  string city,
  string zipCode
}

struct User {
  i64 id,
  string name,
  string email,
  Address address
}
```

**Field Modifiers:**

```thrift
struct CreateUserRequest {
  required string name,      // Must be provided
  optional string email,     // May be null
  string phone               // Optional by default
}
```

**Field Indices:**

```thrift
struct User {
  1: i64 id,
  2: string name,
  3: string email
}
```

Field indices are parsed but not currently used for ordering. They serve as
documentation for field evolution.

#### Exceptions

Typed error responses that integrate with service methods:

```thrift
exception ValidationError {
  i32 code,
  string message,
  list<string> fieldErrors
}

exception NotFoundError {
  i32 code,
  string message
}
```

**Generated Code:**

- **C#**: Class inheriting from `Exception`
- **TypeScript**: Interface (part of union discriminated types)

#### Unions

Discriminated unions for type-safe result handling:

```thrift
union UserResult {
  User success,
  NotFoundError notFound,
  ValidationError validationError
}

service UserService {
  UserResult getUser(i64 id)
}
```

**Generated Code:**

- **C#**: Class with discriminator enum and nullable fields
- **TypeScript**: Discriminated union type with `type` field

**Usage Example (TypeScript):**

```typescript
const result = await client.getUser(123);

if (result.type === "success") {
  console.log(result.success.name);
} else if (result.type === "notFound") {
  console.error(result.notFound.message);
}
```

#### Enums

Named integer constants with auto-increment support:

Recommended style: use `PascalCase` for enum member names in Thrift (for example
`NearlyDone`).

```thrift
enum Status {
  Pending = 1,
  Active = 2,
  Completed = 3
}

// Auto-increment from 0
enum Priority {
  Low,      // 0
  Medium,   // 1
  High,     // 2
  Urgent    // 3
}

// Mixed explicit and auto-increment
enum Color {
  Red = 1,
  Green,    // 2
  Blue = 10,
  Yellow    // 11
}
```

**Features:**

- Explicit integer values
- Auto-increment (default starts at 0)
- Mixed explicit and auto-increment
- Negative values supported
- Documentation comments per value

**Example:**

```thrift
/**
 * Task priority levels
 */
enum TaskPriority {
  /**
   * Low priority tasks
   */
  Low = 0,
  Normal = 1,
  High = 2,
  Critical = 3
}
```

## Namespaces

Namespaces map Thrift types to target language packages or namespaces:

```thrift
namespace cs MyProject.Api.Generated
namespace ts myproject.api
```

**Supported Scopes:**

- `cs` - C# namespace
- `ts` - TypeScript module path
- Custom scopes for other generators

**Example:**

```thrift
namespace cs MyCompany.UserService.Generated
namespace ts mycompany.userservice

service UserService {
  // Service definition
}
```

**Rules:**

- One namespace per scope per file
- Namespaces apply to all types in the file
- Must be defined before any type definitions

## Services

Services define RPC interfaces with typed methods:

```thrift
service Calculator {
  i32 add(i32 a, i32 b)
  i32 subtract(i32 a, i32 b)
  double divide(double numerator, double denominator)
}
```

### Service Attributes

Customize service behavior with attributes:

```thrift
service UserService {
  User getUser(i64 id)
} (http.baseUrl = "/api/users")
```

**Supported Attributes:**

| Attribute      | Description                       | Example        |
| -------------- | --------------------------------- | -------------- |
| `http.baseUrl` | Base URL path for all HTTP routes | `"/api/users"` |

### Functions

Service methods define the RPC contract:

```thrift
service UserService {
  // Simple function
  User getUser(i64 id)
  
  // Multiple parameters
  User createUser(string name, string email)
  
  // Struct parameters
  User updateUser(UpdateUserRequest request)
  
  // Returns list
  list<User> searchUsers(string query)
  
  // Void return
  void deleteUser(i64 id)
}
```

### Function Attributes

Customize individual function behavior:

```thrift
service UserService {
  // Use HTTP GET instead of POST
  User getUser(i64 id) (http.method = "get")
  
  // POST is default
  User createUser(CreateUserRequest request)
}
```

**Supported Attributes:**

| Attribute     | Description                   | Values            |
| ------------- | ----------------------------- | ----------------- |
| `http.method` | HTTP method for this function | `"get"`, `"post"` |

**Behavior:**

- **POST (default)**: Parameters sent in request body as JSON
- **GET**: Parameters sent as query parameters (supports nested objects via
  flattening)

### Throws Clause

Declare typed exceptions that functions may throw:

```thrift
exception ValidationError {
  i32 code,
  string message
}

exception NotFoundError {
  i32 code,
  string message
}

service UserService {
  User getUser(i64 id) throws (NotFoundError notFound)
  
  User createUser(CreateUserRequest request) throws (
    ValidationError validationError,
    DuplicateError duplicateError
  )
}
```

**Generated Code:**

- **C#**: Method signature includes exception type in documentation; exceptions
  are thrown normally
- **TypeScript**: Not directly used (use union return types for type-safe error
  handling)

**Best Practice:**

For type-safe error handling in both C# and TypeScript, use union return types:

```thrift
union UserResult {
  User success,
  NotFoundError notFound
}

service UserService {
  UserResult getUser(i64 id)
}
```

## Documentation Comments

Add rich documentation to all types using `/** ... */` syntax:

```thrift
/**
 * User management service
 * 
 * Handles CRUD operations for user accounts.
 */
service UserService {
  /**
   * Retrieves a user by ID
   * 
   * @param id The unique user identifier
   * @return The user object or an error
   */
  UserResult getUser(i64 id)
}
```

**Features:**

- Full Markdown support preserved in generated code
- Multi-line comments
- Generates JSDoc in TypeScript
- Generates XML documentation in C#
- Works on: services, functions, structs, exceptions, unions, enums, enum values

**Example with Markdown:**

````thrift
/**
 * # User Profile
 * 
 * Represents a user account in the system.
 * 
 * ## Fields
 * - `id`: Unique identifier
 * - `name`: Display name
 * - `email`: Contact email
 * 
 * ## Usage
 * ```typescript
 * const user = await client.getUser(123);
 * console.log(user.name);
 * ```
 */
struct User {
  /**
   * Unique user identifier (primary key)
   */
  i64 id,
  
  /**
   * User's display name
   * 
   * Must be 3-50 characters
   */
  string name,
  
  string email
}
````

## Complete Example

Here's a complete Thrift IDL file demonstrating all major features:

```thrift
// Namespace declarations
namespace cs MyCompany.Calculator.Generated
namespace ts mycompany.calculator

/**
 * Generic error response
 */
exception GenericError {
  required i32 code,
  required string message
}

/**
 * Result type for calculator operations
 * 
 * Returns either the calculated result or an error.
 */
union NumberResult {
  i64 success,
  GenericError genericError
}

/**
 * Request parameters for calculator operations
 */
struct NumberRequestParams {
  /**
   * The first number to use
   */
  required i64 a,
  
  /**
   * The second number to use
   */
  required i64 b
}

/**
 * Calculator operations service
 * 
 * Provides basic arithmetic operations with error handling.
 */
service Calculator {
  /**
   * Adds two numbers together
   * 
   * The result of `a + b` will be returned.
   * 
   * @param requestParams The two numbers to add
   * @return Union containing either the sum or an error
   */
  NumberResult add(NumberRequestParams requestParams) throws (GenericError genericError),
  
  /**
   * Add two numbers using query parameters
   * 
   * Demonstrates GET method with multiple parameters.
   */
  i32 addTwoNumbers(i32 a, i32 b) (http.method = "get"),
  
  /**
   * Subtract two numbers
   * 
   * The result of `a - b` will be returned.
   */
  NumberResult subtract(NumberRequestParams requestParams) throws (GenericError genericError),
  
  /**
   * Get all numbers between `a` and `b`
   * 
   * Returns a list that can be streamed for large ranges.
   * 
   * @param requestParams Contains start (a) and end (b) values
   * @return List of integers in the range [a, b]
   */
  list<i64> range(NumberRequestParams requestParams)
} (http.baseUrl = "/api/calculator")
```

This example demonstrates:

- Namespaces for C# and TypeScript
- Exception definition with required fields
- Union type for type-safe result handling
- Struct with documented fields
- Service with documentation
- Functions with various signatures
- Throws clause
- HTTP method customization (`http.method = "get"`)
- Service-level attribute (`http.baseUrl`)
- List return type (supports streaming)
- Rich documentation comments with Markdown

## Limitations

ContractForge focuses on single-file service definitions for the MVP. The
following standard Thrift features are **not currently supported** but are
planned for future releases:

### Planned for Future Releases

**Includes** ❌

Cross-file imports and type references are not supported:

```thrift
// NOT SUPPORTED
include "common.thrift"
include "types.thrift" as Types
```

**Workaround**: Consolidate all type definitions in a single `.thrift` file.

**Service Inheritance/Extends** ❌

Services cannot extend other services:

```thrift
// NOT SUPPORTED
service ExtendedCalculator extends Calculator {
  i32 multiply(i32 a, i32 b)
}
```

**Workaround**: Define all methods in a single service.

**Struct Inheritance/Extends** ❌

Structs cannot extend other structs:

```thrift
// NOT SUPPORTED
struct AdminUser extends User {
  list<string> permissions
}
```

**Workaround**: Duplicate fields or use composition with nested structs.

### Not Planned for MVP

The following features are not in the current roadmap:

| Feature              | Status      | Workaround                           |
| -------------------- | ----------- | ------------------------------------ |
| `typedef`            | Not planned | Use the original type directly       |
| `const`              | Not planned | Define constants in generated code   |
| `set<T>`             | Not planned | Use `list<T>` and enforce uniqueness |
| `senum`              | Not planned | Use regular `enum`                   |
| Default field values | Not planned | Set defaults in implementation       |
| `oneway` functions   | Not planned | Use void return type                 |

## Best Practices

### Single-File Organization

Keep all types, services, and exceptions in one `.thrift` file:

```thrift
// ✅ GOOD: Everything in one file
namespace cs MyService.Generated

exception ServiceError { /* ... */ }
struct Request { /* ... */ }
struct Response { /* ... */ }
service MyService { /* ... */ }
```

### Use Unions for Error Handling

Prefer union return types over throws clauses for type-safe error handling:

```thrift
// ✅ GOOD: Type-safe errors
union UserResult {
  User success,
  NotFoundError notFound,
  ValidationError validationError
}

service UserService {
  UserResult getUser(i64 id)
}

// ⚠️ OK: But less type-safe in TypeScript
service UserService {
  User getUser(i64 id) throws (NotFoundError err)
}
```

### Document Your API

Use documentation comments extensively:

```thrift
/**
 * User management service
 * 
 * Handles all user-related operations including:
 * - User creation and updates
 * - Authentication
 * - Profile management
 */
service UserService {
  /**
   * Creates a new user account
   * 
   * @param request User details including name and email
   * @return The created user or validation errors
   */
  UserResult createUser(CreateUserRequest request)
}
```

### HTTP Method Selection

Choose HTTP methods based on semantics:

```thrift
service UserService {
  // GET for read operations (parameters in query string)
  User getUser(i64 id) (http.method = "get")
  list<User> listUsers(string filter) (http.method = "get")
  
  // POST for write operations (parameters in body)
  User createUser(CreateUserRequest request)
  User updateUser(UpdateUserRequest request)
  void deleteUser(i64 id)
}
```

### Struct vs Multiple Parameters

**Always use a single struct parameter for POST methods.** ASP.NET Core model
binding only supports binding one parameter from the request body. Multiple
parameters will cause unexpected behavior.

For GET methods, multiple parameters work correctly as they're bound from query
strings.

```thrift
// ✅ GOOD: Single struct parameter (works for both GET and POST)
struct SearchRequest {
  string query,
  i32 limit,
  i32 offset,
  list<string> filters
}

service SearchService {
  list<Result> search(SearchRequest request)
}

// ✅ GOOD: Multiple parameters with GET
service SearchService {
  list<Result> simpleSearch(string query, i32 limit) (http.method = "get")
}

// ❌ BAD: Multiple parameters with POST (will fail in .NET)
service SearchService {
  list<Result> search(string query, i32 limit, i32 offset, list<string> filters)
  // ASP.NET Core cannot bind multiple parameters from request body!
}
```

**Recommendation:** Use a single struct parameter for all methods to avoid
binding issues and make your API easier to extend.

### Streaming Large Datasets

Use `list<T>` return types for datasets that may be large:

```thrift
service DataService {
  // Supports streaming in generated code
  list<Record> getAllRecords()
  
  // Client can choose:
  // - JSON: const records = await client.getAllRecords()
  // - NDJSON: for await (const record of client.getAllRecordsStream())
}
```

## See Also

- [C# JSON API Generator](../generators/csharp-jsonapi.md) - Generate ASP.NET
  Core server code from Thrift IDL
- [TypeScript Client Generator](../generators/typescript-client.md) - Generate
  type-safe TypeScript clients
- [Getting Started Guide](../getting-started/quickstart.md) - Build your first
  ContractForge API
- [CLI Reference](cli.md) - Command-line tool options and usage
