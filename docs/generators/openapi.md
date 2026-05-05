# OpenAPI Generator

The `openapi` generator creates an OpenAPI JSON document from Thrift IDL. It
does not introduce a new schema language; the Thrift file remains the source of
truth.

## Quick Start

```bash
contractforge --entry service.thrift --generator openapi --output openapi.json
```

The default output is OpenAPI 3.0.3 JSON.

## Options

Generator options are provided with `-O key=value`.

| Option          | Default             | Description                         |
| --------------- | ------------------- | ----------------------------------- |
| `openapi`       | `3.0.3`             | OpenAPI version: `3.0.3` or `3.1.0` |
| `info.title`    | `ContractForge API` | OpenAPI `info.title`                |
| `info.version`  | `1.0.0`             | OpenAPI `info.version`              |
| `servers.0.url` | none                | Optional first server URL           |

```bash
contractforge \
  --entry service.thrift \
  --generator openapi \
  --output openapi.json \
  -O openapi=3.1.0 \
  -O info.title="Users API" \
  -O info.version=1.2.0 \
  -O servers.0.url=https://api.example.com
```

Unknown options are rejected.

## Routes

Paths match the same route helpers used by the C# and TypeScript generators.

```thrift
service User {
  UserProfile getProfile(i64 id) (http.method = "get")
} (http.baseUrl = "/api/users")
```

This emits a path like:

```text
/api/users/user-service/get-profile
```

The `http.baseUrl` value is normalized, so `api/users`, `/api/users`, and
`/api/users/` produce the same base path.

## Requests

Functions default to `POST`. A function can opt into `GET` with `http.method`.

```thrift
service User {
  UserProfile getProfile(i64 id) (http.method = "get")
  UserProfile createProfile(CreateProfileRequest request)
}
```

GET parameters are emitted as query parameters. Struct parameters are flattened
with dotted names.

```thrift
struct AddressFilter {
  1: required string city
}

struct UserFilter {
  1: required string name,
  2: optional AddressFilter address
}

service User {
  list<UserProfile> search(UserFilter filter) (http.method = "get")
}
```

This emits query parameters such as `filter.name` and `filter.address.city`.

POST functions can have zero or one parameter. POST functions with multiple
parameters produce a generator error, matching the C# JSON API generator. Wrap
multiple request values in a Thrift struct.

## Schemas

The generator emits component schemas for:

- `struct`
- `exception`
- `enum`
- `union`

Schema component names use the exact Thrift identifiers. If two input documents
define the same schema name, generation fails with an error.

Optional fields are emitted as non-required properties. In OpenAPI 3.0.3 they
use `nullable: true`; in OpenAPI 3.1.0 they use JSON Schema nullable types or
`oneOf` for references.

## Responses

Non-void functions emit a `200` response with `application/json` content.

Void functions emit a `204` response with no body.

Functions returning `list<T>` emit two response media types:

- `application/json` for the full array
- `application/x-ndjson` for streaming items

Functions with `throws` clauses emit a `default` response with an
`application/json` schema using `oneOf` over the declared exception schemas.

## Authorization

The `authorize.policy` attribute is emitted as a vendor extension named
`x-authorize-policy`.

```thrift
service User {
  UserProfile getProfile(i64 id) (http.method = "get", authorize.policy = "CanViewUsers")
} (authorize.policy = "AuthenticatedUsers")
```

The OpenAPI output does not assume bearer tokens, cookies, or API keys. Add a
security scheme downstream if your runtime has a standard auth mechanism.
