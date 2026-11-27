# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [0.0.0-alpha.3] - 2026-11-27

### Added

- Install and run the same `ContractForge.Cli` tool on .NET 9, 10, or 11. The
  tool targets .NET 9 and rolls forward to later stable runtimes without
  `--allow-roll-forward`.
- Generate an OpenAPI JSON document with `--generator openapi` (OpenAPI 3.0.3 by
  default, with 3.1.0 available).
- Generated TypeScript clients accept a `credentials` option for both JSON
  requests and NDJSON streams.
- Check the installed CLI version with `contractforge --version` or `-v`.
  Mistyped generator names now show the available generators and suggest a close
  match.

### Fixed

- Generated C# unions now deserialize JSON regardless of property order and
  reject payloads that do not match the union discriminator.
- Leading or trailing slashes in `http.baseUrl` no longer produce malformed
  service routes.

### Upgrade guide

- Regenerated TypeScript clients now include the Thrift parameter name in GET
  queries for a single request struct. For example, with a parameter named
  `request`, `paginationParams.page=2` becomes
  `request.paginationParams.page=2`. Check servers, mocks, and tests that expect
  the alpha.2 query keys.
- Install with
  `dotnet tool install --global ContractForge.Cli --version 0.0.0-alpha.3`. If
  the tool is already installed, use `dotnet tool update` with the same flags.

## [0.0.0-alpha.2] - 2026-03-19

### Fixed

- C# exception generation now handles exceptions with only a `message` field,
  avoiding invalid constructor signatures in generated code.

## [0.0.0-alpha.1] - 2026-03-05

### Added

- TypeScript generators now support enums.
- C# generator now supports emitting partial classes for extensibility.

### Changed

- C# enum generation now preserves PascalCase identifiers and emits
  `[JsonStringEnumMemberName("<camelCase>")]` for JSON serialization.

### Fixed

- TypeScript client now resolves headers correctly.
- Optional fields in TypeScript generators are now emitted with `?`.

### Upgrade guide

- If your Thrift enum values were `ALL_CAPS`, update them to PascalCase (e.g.,
  `Pending`, `Active`, `Completed`) to match the new generator behavior.
- If any clients/tests relied on the old serialized names, update those payloads
  or add a custom converter.

[0.0.0-alpha.3]: https://github.com/AdeAttwood/ContractForge/compare/v0.0.0-alpha.2...v0.0.0-alpha.3
[0.0.0-alpha.2]: https://github.com/AdeAttwood/ContractForge/compare/v0.0.0-alpha.1...v0.0.0-alpha.2
[0.0.0-alpha.1]: https://github.com/AdeAttwood/ContractForge/compare/v0.0.0-alpha...v0.0.0-alpha.1
