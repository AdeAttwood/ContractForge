# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

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

[0.0.0-alpha.1]: https://github.com/AdeAttwood/ContractForge/compare/v0.0.0-alpha...v0.0.0-alpha.1
