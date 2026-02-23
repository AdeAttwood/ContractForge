# TypeScript Client Generator

The `typescript-client` generator creates type-safe, standards-compliant
TypeScript clients from Thrift IDL files, enabling seamless RPC communication
across Node.js, Deno, Bun, and browsers.

## Overview

This generator produces:

- **Type-safe TypeScript interfaces** - DTOs for structs, exceptions, and unions
- **Client classes** - Fetch-based HTTP clients with full type inference
- **Dual methods for lists** - Regular (JSON) and streaming (NDJSON) methods
- **Helper utilities** - Query parameter flattening and URL building
- **Zero dependencies** - Uses only web standards APIs

## Features

- ✅ Fetch API-based HTTP client (web standards)
- ✅ Full TypeScript type safety with inference
- ✅ Discriminated unions for type-safe error handling
- ✅ Streaming support with AsyncIterable for large datasets
- ✅ Dual method pattern: regular (JSON) and streaming (NDJSON)
- ✅ Runtime compatibility: Node.js 18+, Deno, Bun, browsers
- ✅ Zero external dependencies
- ✅ JSDoc comment preservation
- ✅ Query parameter flattening for nested objects

## Quick Start

Build your first TypeScript client in 5 minutes. This guide shows you how to
call a Calculator API from Node.js, Deno, Bun, or the browser.

### Prerequisites

- NetRpc CLI installed
- Node.js 18+, Deno, or Bun
- A running NetRpc server (see [C# Server Guide](csharp-jsonapi.md#quick-start))

### Step 1: Generate Client Code

```bash
contractforge generate typescript-client -i calculator.thrift -o client.ts
```

This creates a `client.ts` file with:

- `CalculatorClient` class
- Type-safe method signatures
- Streaming support

### Step 2: Create Your Script

Create `test.ts`:

```typescript
import { CalculatorClient } from "./client.ts";

const client = new CalculatorClient({
  host: "http://localhost:5000",
});

// Add two numbers
const sum = await client.add({ a: 5, b: 3 });
console.log(`5 + 3 = ${sum}`); // Output: 5 + 3 = 8

// Subtract two numbers
const difference = await client.subtract({ a: 10, b: 4 });
console.log(`10 - 4 = ${difference}`); // Output: 10 - 4 = 6
```

### Step 3: Run It

**With Deno:**

```bash
deno run --allow-net test.ts
```

**With Node.js:**

```bash
node test.ts  # If using ES modules
```

**With Bun:**

```bash
bun run test.ts
```

### Output

```
5 + 3 = 8
10 - 4 = 6
```

### What You've Built

- ✅ Type-safe API client
- ✅ Full TypeScript inference
- ✅ Works in Node.js, Deno, Bun, and browsers
- ✅ Zero external dependencies

### Next Steps

- Learn about [streaming support](#streaming-support) for large datasets
- Explore [error handling](#error-handling) with unions
- See [format negotiation](#format-negotiation) for JSON vs NDJSON
- Review [type safety](#type-safety) features

## Generated Code Structure

### Data Transfer Objects (DTOs)

Thrift structs become TypeScript interfaces with full JSDoc preservation.

**Thrift:**

```thrift
/**
 * User account information
 */
struct User {
  /**
   * Unique user identifier
   */
  1: i64 id,
  /**
   * User's display name
   */
  2: string name,
  3: string? email
}
```

**Generated TypeScript:**

```typescript
/**
 * User account information
 */
export interface User {
  /**
   * Unique user identifier
   */
  id: number;
  /**
   * User's display name
   */
  name: string;
  email?: string;
}
```

### Exceptions

Thrift exceptions become TypeScript interfaces, similar to structs.

**Thrift:**

```thrift
exception NotFoundError {
  i32 code,
  string message
}
```

**Generated TypeScript:**

```typescript
export interface NotFoundError {
  code: number;
  message: string;
}
```

### Union Types

Thrift unions become discriminated unions with a `type` field for type
narrowing.

**Thrift:**

```thrift
union UserResult {
  User success,
  NotFoundError notFoundError,
  ValidationError validationError
}
```

**Generated TypeScript:**

```typescript
export type UserResult =
  | { type: "success"; success: User }
  | { type: "notFoundError"; notFoundError: NotFoundError }
  | { type: "validationError"; validationError: ValidationError };
```

**Usage with Type Narrowing:**

```typescript
const result = await client.getUser({ id: 123 });

if (result.type === "success") {
  // TypeScript knows result.success is User
  console.log(result.success.name);
} else if (result.type === "notFoundError") {
  // TypeScript knows result.notFoundError is NotFoundError
  console.error(result.notFoundError.message);
}
```

### Client Class

**Thrift:**

```thrift
service UserService {
  User getUser(i64 id)
  list<User> getAllUsers()
}
```

**Generated TypeScript:**

```typescript
export interface UserServiceClientOptions {
  host: string;
}

/**
 * User service client
 */
export class UserServiceClient {
  public constructor(private options: UserServiceClientOptions) {}

  // Helper methods (private)
  private buildUrl(path: string, query: Record<string, unknown>) {/* ... */}
  private serialize(obj: unknown) {/* ... */}
  private async request(path: string, method: "GET" | "POST", params: unknown) {
    /* ... */
  }
  private async *streamNdjson<T>(
    path: string,
    method: "GET" | "POST",
    params: unknown,
  ): AsyncIterable<T> {/* ... */}

  // Service methods (public)
  public async getUser(id: number): Promise<User> {
    return await this.request(
      "/rpc/user-service/get-user",
      "POST",
      { id },
    );
  }

  // Regular method for list returns (JSON)
  public async getAllUsers(): Promise<Array<User>> {
    return await this.request(
      "/rpc/user-service/get-all-users",
      "POST",
      {},
    );
  }

  // Streaming method for list returns (NDJSON)
  public async *getAllUsersStream(): AsyncIterable<User> {
    yield* this.streamNdjson<User>(
      "/rpc/user-service/get-all-users",
      "POST",
      {},
    );
  }
}
```

### Helper Utilities

The generator includes several helper utilities in each client class:

**flatten() - Query Parameter Flattening:**

Converts nested objects to flat key-value pairs for query strings:

```typescript
flatten({ user: { name: "John", age: 30 } });
// → { "user.name": "John", "user.age": "30" }
```

This enables complex objects to be sent as GET query parameters:

```
/api/endpoint?user.name=John&user.age=30
```

**buildUrl() - URL Construction:**

Constructs URLs with query parameters:

```typescript
buildUrl("/api/users", { id: 123, filter: "active" });
// → http://localhost:5000/api/users?id=123&filter=active
```

**serialize() - JSON Serialization:**

Converts objects to JSON strings for request bodies:

```typescript
serialize({ name: "John", age: 30 });
// → '{"name":"John","age":30}'
```

**request() - Standard HTTP Requests:**

Makes HTTP requests with JSON serialization:

```typescript
await request("/api/users", "POST", { name: "John" });
// → Sends POST with Accept: application/json
```

**streamNdjson() - NDJSON Streaming:**

Makes streaming requests with NDJSON parsing:

```typescript
for await (const item of streamNdjson("/api/users", "POST", {})) {
  // Process items as they arrive
}
// → Sends POST with Accept: application/x-ndjson
```

## Type System

Complete mapping of Thrift types to TypeScript:

| Thrift Type | TypeScript Type     | Example                           |
| ----------- | ------------------- | --------------------------------- |
| `i32`       | `number`            | `42`                              |
| `i64`       | `number`            | `9007199254740991`                |
| `double`    | `number`            | `3.14159`                         |
| `string`    | `string`            | `"Hello"`                         |
| `list<T>`   | `Array<T>`          | `[1, 2, 3]`                       |
| `map<K, V>` | `Record<K, V>`      | `{ "key": "value" }`              |
| `struct`    | `interface`         | `{ id: 1, name: "John" }`         |
| `exception` | `interface`         | `{ code: 404, message: "..." }`   |
| `union`     | discriminated union | `{ type: "success", success: 1 }` |

### Notes on Type Mappings

**Integer Types:**

All Thrift integer types (`i32`, `i64`) map to JavaScript `number`. JavaScript
numbers are 64-bit floats (IEEE 754), providing safe integer precision up to
`Number.MAX_SAFE_INTEGER` (2^53 - 1 = 9,007,199,254,740,991).

If you need integers larger than this, consider:

- Using `string` type on both client and server
- Using `double` and accepting floating-point representation
- Splitting large values into smaller components

**Maps:**

TypeScript `Record<K, V>` is a type alias for objects with string keys. When
using non-string keys in Thrift (like `map<i32, string>`), JSON serialization
will convert the keys to strings, but TypeScript will preserve the key type for
type checking.

## Client Usage

### Instantiation

Create a client instance with configuration:

```typescript
const client = new UserServiceClient({
  host: "http://localhost:5000",
});
```

**Multiple Environments:**

```typescript
const dev = new UserServiceClient({ host: "http://localhost:5000" });
const prod = new UserServiceClient({ host: "https://api.example.com" });
```

### Making Requests

**Single Parameter:**

```typescript
// Thrift: User getUser(i64 id)
const user = await client.getUser(123);
```

**Multiple Parameters:**

```typescript
// Thrift: i32 add(i32 a, i32 b)
const result = await client.add(5, 3); // → 8
```

**Struct Parameters:**

```typescript
// Thrift: User createUser(UserRequest request)
const user = await client.createUser({
  name: "John Doe",
  email: "john@example.com",
});
```

### Error Handling

**Union-Based Errors:**

When a Thrift function returns a union, use type narrowing:

```typescript
const result = await client.createUser({ name: "John" });

if (result.type === "success") {
  console.log("User created:", result.success.id);
} else if (result.type === "validationError") {
  console.error("Validation failed:", result.validationError.message);
} else if (result.type === "duplicateError") {
  console.error("User already exists:", result.duplicateError.message);
}
```

**Network Errors:**

Fetch errors (network failures, timeouts) throw exceptions:

```typescript
try {
  const user = await client.getUser(123);
} catch (error) {
  console.error("Network error:", error);
}
```

**HTTP Errors:**

For non-union returns, HTTP errors are caught and returned:

```typescript
const result = await client.getUser(123);
// If server returns 404, result contains error information
```

### Type Safety

TypeScript provides full type inference:

```typescript
const user = await client.getUser(123);
// user is typed as User
console.log(user.name); // ✅ Autocomplete works

const result = await client.createUser({ name: "John" });
if (result.type === "success") {
  // result.success is typed as User
  console.log(result.success.id); // ✅ Type narrowing works
}
```

**IDE Support:**

- Full autocomplete for method names
- Parameter type checking
- Return type inference
- JSDoc hover documentation

## Streaming Support

For methods that return `list<T>` in Thrift, the generator creates **two
methods**: a regular method returning `Promise<Array<T>>` and a streaming method
returning `AsyncIterable<T>`.

### Overview: Dual Method Pattern

**Regular Method:**

- Name: `methodName()`
- Returns: `Promise<Array<T>>`
- Format: JSON (`Accept: application/json`)
- Use case: Small datasets, need complete array

**Streaming Method:**

- Name: `methodNameStream()`
- Returns: `AsyncIterable<T>`
- Format: NDJSON (`Accept: application/x-ndjson`)
- Use case: Large datasets, progressive processing

### Regular Methods (JSON)

Returns the complete array after all items are received.

**Signature:**

```typescript
public async getUsers(): Promise<Array<User>>
```

**Usage:**

```typescript
const users = await client.getUsers();
console.log(`Total users: ${users.length}`);

for (const user of users) {
  console.log(user.name);
}
```

**Characteristics:**

- ✅ Simple array API
- ✅ Can use array methods (map, filter, etc.)
- ✅ Get length immediately
- ⚠️ Buffers entire response in memory
- ⚠️ Must wait for all data before processing

**Example with Array Operations:**

```typescript
const users = await client.getUsers();

// Filter, map, reduce - all available
const activeUsers = users.filter((u) => u.active);
const names = users.map((u) => u.name);
const total = users.reduce((sum, u) => sum + u.score, 0);
```

### Streaming Methods (NDJSON)

Returns items one at a time as they arrive from the server.

**Signature:**

```typescript
public async *getUsersStream(): AsyncIterable<User>
```

**Usage:**

```typescript
for await (const user of client.getUsersStream()) {
  // Process each user as it arrives
  console.log(user.name);
}
```

**Characteristics:**

- ✅ Memory efficient (no buffering)
- ✅ Process items immediately as they arrive
- ✅ Lower time-to-first-byte
- ✅ Supports infinite/very large streams
- ⚠️ Cannot use array methods directly
- ⚠️ Don't know total count until end

**Example with Progressive Display:**

```typescript
let count = 0;
for await (const user of client.getUsersStream()) {
  count++;
  console.log(`[${count}] ${user.name}`);
  // Update UI progressively
}
console.log(`Total: ${count} users`);
```

**Example with Early Exit:**

```typescript
for await (const user of client.getUsersStream()) {
  if (user.name === "John Doe") {
    console.log("Found John Doe!");
    break; // Stop processing remaining items
  }
}
```

### Comparison: JSON vs NDJSON

| Aspect            | JSON (Regular)          | NDJSON (Streaming)     |
| ----------------- | ----------------------- | ---------------------- |
| **Return Type**   | `Promise<Array<T>>`     | `AsyncIterable<T>`     |
| **Accept Header** | `application/json`      | `application/x-ndjson` |
| **Memory Usage**  | Buffers entire array    | Streams item-by-item   |
| **Time to First** | Wait for all items      | First item immediately |
| **Array Methods** | ✅ map, filter, reduce  | ❌ Must collect first  |
| **Length/Count**  | ✅ Immediate            | ❌ Only after complete |
| **Early Exit**    | ❌ Already received all | ✅ Stop fetching       |
| **Best For**      | < 1,000 items           | > 1,000 items          |

### When to Use Each

**Use Regular Methods (JSON) When:**

- Dataset is small (< 1,000 items)
- You need the complete array before processing
- You want to use array methods (map, filter, reduce)
- You need the total count immediately
- Simplicity is preferred

**Use Streaming Methods (NDJSON) When:**

- Dataset is large (> 1,000 items)
- Memory efficiency matters
- You want progressive/incremental display
- You might exit early (searching, limiting)
- Time-to-first-byte is important
- Dataset size is unknown or unbounded

### Memory Considerations

**Regular Method Memory Usage:**

```typescript
// Loads ALL users into memory at once
const users = await client.getUsers(); // Could be 100MB+
```

For 10,000 users at ~10KB each = ~100MB in memory.

**Streaming Method Memory Usage:**

```typescript
// Only ONE user in memory at a time
for await (const user of client.getUsersStream()) {
  await processUser(user); // ~10KB per iteration
}
```

Memory usage stays constant regardless of dataset size.

### Error Handling in Streams

**Network Errors:**

```typescript
try {
  for await (const user of client.getUsersStream()) {
    console.log(user.name);
  }
} catch (error) {
  console.error("Stream error:", error);
  // Partial results already processed
}
```

**Partial Results:**

Streaming methods process items as they arrive. If an error occurs mid-stream,
items processed before the error are not rolled back.

```typescript
let processed = 0;
try {
  for await (const user of client.getUsersStream()) {
    await saveToDatabase(user);
    processed++;
  }
  console.log(`Successfully processed ${processed} users`);
} catch (error) {
  console.error(`Failed after processing ${processed} users`);
  // First ${processed} users are already saved
}
```

**Collecting Streaming Results:**

If you need to collect all items from a stream:

```typescript
const users: User[] = [];
for await (const user of client.getUsersStream()) {
  users.push(user);
}
// Now users is an array, but you've lost streaming benefits
```

## Format Negotiation

The generator automatically sets the appropriate `Accept` header based on the
method called.

### JSON Format (Regular Methods)

Regular methods set `Accept: application/json`:

```typescript
const users = await client.getUsers();
// HTTP Request:
// Accept: application/json
//
// HTTP Response:
// Content-Type: application/json
// [{"id":1,"name":"Alice"},{"id":2,"name":"Bob"}]
```

### NDJSON Format (Streaming Methods)

Streaming methods set `Accept: application/x-ndjson`:

```typescript
for await (const user of client.getUsersStream()) {
  console.log(user);
}
// HTTP Request:
// Accept: application/x-ndjson
//
// HTTP Response:
// Content-Type: application/x-ndjson
// {"id":1,"name":"Alice"}
// {"id":2,"name":"Bob"}
```

### Server Configuration

The server must support format negotiation. See the
[C# JSON API Generator](csharp-jsonapi.md#format-negotiation) documentation for
server setup.

### Testing with curl

**JSON Request:**

```bash
curl http://localhost:5000/rpc/user-service/get-users \
  -H "Accept: application/json" \
  -H "Content-Type: application/json" \
  -d '{}'
```

**NDJSON Request:**

```bash
curl http://localhost:5000/rpc/user-service/get-users \
  -H "Accept: application/x-ndjson" \
  -H "Content-Type: application/json" \
  -d '{}'
```

## Runtime Compatibility

The generated TypeScript client uses only web standards APIs, ensuring
compatibility across all modern JavaScript runtimes.

### Fetch API

All runtimes support the Fetch API:

- **Node.js 18+**: Built-in `fetch()` (no polyfill needed)
- **Node.js < 18**: Use `node-fetch` or upgrade
- **Deno**: Native `fetch()` support
- **Bun**: Native `fetch()` support
- **Browsers**: Universal support (Chrome 42+, Firefox 39+, Safari 10.1+)

### ReadableStream

NDJSON streaming uses `ReadableStream`:

- **Node.js 18+**: Full support
- **Deno**: Full support
- **Bun**: Full support
- **Browsers**: Full support (Chrome 43+, Firefox 65+, Safari 10.1+)

### TypeScript Configuration

Recommended `tsconfig.json` settings:

**For Node.js:**

```json
{
  "compilerOptions": {
    "target": "ES2022",
    "module": "ESNext",
    "moduleResolution": "node",
    "lib": ["ES2022", "DOM"],
    "strict": true,
    "esModuleInterop": true
  }
}
```

**For Deno:**

```json
{
  "compilerOptions": {
    "target": "ES2022",
    "lib": ["deno.window"],
    "strict": true
  }
}
```

**For Browsers:**

```json
{
  "compilerOptions": {
    "target": "ES2022",
    "module": "ESNext",
    "lib": ["ES2022", "DOM"],
    "strict": true
  }
}
```

### Zero Dependencies

The generated client has **no external dependencies**. It uses only standard
JavaScript/TypeScript features:

- `fetch()` - HTTP requests
- `URL` - URL construction
- `ReadableStream` - Streaming responses
- `TextDecoder` - Text decoding
- `JSON` - JSON parsing

This ensures:

- ✅ Small bundle size
- ✅ No dependency conflicts
- ✅ No security vulnerabilities from dependencies
- ✅ Future-proof code

## HTTP Method Routing

By default, all service methods use **POST** with parameters in the request
body. You can customize this with the `http.method` attribute.

### POST (Default)

**Thrift:**

```thrift
service UserService {
  User getUser(i64 id)
}
```

**Generated TypeScript:**

```typescript
public async getUser(id: number): Promise<User> {
  return await this.request(
    "/rpc/user-service/get-user",
    "POST",
    { id }
  );
}
```

**HTTP Request:**

```http
POST /rpc/user-service/get-user HTTP/1.1
Content-Type: application/json

{"id": 123}
```

### GET with Query Parameters

**Thrift:**

```thrift
service UserService {
  User getUser(i64 id) (http.method = "get")
}
```

**Generated TypeScript:**

```typescript
public async getUser(id: number): Promise<User> {
  return await this.request(
    "/rpc/user-service/get-user",
    "GET",
    { id }
  );
}
```

**HTTP Request:**

```http
GET /rpc/user-service/get-user?id=123 HTTP/1.1
```

### Query Parameter Flattening

The `flatten()` utility converts nested objects to flat query parameters:

**Nested Object:**

```typescript
await client.searchUsers({
  filter: {
    name: "John",
    age: 30,
  },
  sort: "name",
});
```

**Generated URL:**

```
/rpc/user-service/search-users?filter.name=John&filter.age=30&sort=name
```

**How it Works:**

```typescript
flatten({ filter: { name: "John", age: 30 }, sort: "name" })
// →
{
  "filter.name": "John",
  "filter.age": "30",
  "sort": "name"
}
```

The server (ASP.NET Core) automatically binds these flattened parameters back to
nested objects.

## Working Example

See the complete working example in `samples/DenoTypescriptClient/`:

### File Structure

```
samples/DenoTypescriptClient/
├── generated/
│   └── calculator-service.gen.ts    # Generated client
├── main.test.ts                     # Integration tests
└── deno.json                        # Deno configuration
```

### Thrift IDL

**File:** `samples/DotnetJsonWebApi/calculator.thrift`

```thrift
service Calculator {
  NumberResult add(NumberRequestParams requestParams),
  i32 addTwoNumbers(i32 a, i32 b) (http.method = "get"),
  list<i64> range(NumberRequestParams requestParams)
}
```

### Generated Client Usage

**File:** `samples/DenoTypescriptClient/main.test.ts`

```typescript
import { CalculatorClient } from "./generated/calculator-service.gen.ts";

const client = new CalculatorClient({ host: "http://localhost:5050" });

// Regular method call with union return
const result = await client.add({ a: 1, b: 2 });
if (result.type === "success") {
  console.log(result.success); // → 3
}

// GET method with multiple parameters
const sum = await client.addTwoNumbers(1, 1);
console.log(sum); // → 2

// Regular method returning array (JSON)
const numbers = await client.range({ a: 5, b: 10 });
console.log(numbers); // → [5, 6, 7, 8, 9, 10]

// Streaming method (NDJSON)
for await (const number of client.rangeStream({ a: 5, b: 10 })) {
  console.log(number); // → 5, 6, 7, 8, 9, 10 (one at a time)
}
```

### Running the Example

**Start the Server:**

```bash
cd samples/DotnetJsonWebApi
dotnet run
```

**Run the Tests:**

```bash
cd samples/DenoTypescriptClient
deno test --allow-net --allow-run
```

### Expected Output

```
running 6 tests from ./main.test.ts
Calls the add method ... ok (15ms)
Calls the subtract method ... ok (8ms)
Calls add two numbers ... ok (7ms)
Calls the subtract method with an error ... ok (9ms)
Calls the range with await json ... ok (10ms)
Calls the range with ndjson ... ok (12ms)

ok | 6 passed | 0 failed (61ms)
```

## Reference

### CLI Options

Generate TypeScript client code from Thrift IDL:

```bash
contractforge -e <input.thrift> -g typescript-client -o <output.ts>
```

| Option            | Description                         | Required |
| ----------------- | ----------------------------------- | -------- |
| `-e, --entry`     | Path to the Thrift IDL file         | Yes      |
| `-g, --generator` | Generator name: `typescript-client` | Yes      |
| `-o, --output`    | Output file path                    | Yes      |

**Example:**

```bash
contractforge -e api.thrift -g typescript-client -o generated/client.ts
```

### Thrift Attributes

Attributes that affect generated TypeScript code:

| Attribute      | Applies To        | Effect                                      |
| -------------- | ----------------- | ------------------------------------------- |
| `http.method`  | Service functions | Changes HTTP method (GET/POST)              |
|                |                   | GET → query parameters                      |
|                |                   | POST → body parameters                      |
| `http.baseUrl` | Services          | Changes base URL path (affects all methods) |

**Example:**

```thrift
service UserService {
  User getUser(i64 id) (http.method = "get")
} (http.baseUrl = "/api/users")
```

Generated method calls `/api/users/get-user` with GET.

## See Also

- [C# JSON API Generator](csharp-jsonapi.md) - Generate server code to pair with
  this client
- [Getting Started Guide](../getting-started/quickstart.md) - Complete tutorial
  for building your first API
- [Thrift IDL Reference](../reference/thrift-idl.md) - Complete Thrift syntax
  guide
