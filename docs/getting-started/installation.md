# Installation

This guide will help you install NetRpc and set up your development environment.

<!-- deno-fmt-ignore-start -->

!!! warning "Pre-release Package"
    NetRpc is not yet published to NuGet. Until the official release, you'll
    need to install from source. NuGet distribution is coming soon.

<!-- deno-fmt-ignore-end -->

## Prerequisites

Before installing NetRpc, ensure you have:

- **.NET 8.0 SDK or later** -
  [Download here](https://dotnet.microsoft.com/download)
- **Command-line access** - Terminal, PowerShell, or Command Prompt

For TypeScript client development, you'll also need:

- **Node.js 18+**, **Deno 1.x+**, or **Bun** - Choose your preferred runtime

## Install from Source

Since NetRpc is not yet available on NuGet, you'll need to build and install it
from source:

### Prerequisites for Source Installation

In addition to the prerequisites above, you'll need:

- **Git** - To clone the repository
- **Task** (optional but recommended) -
  [Install Task](https://taskfile.dev/installation/)

### Installation Steps

1. **Clone the repository**:

   ```bash
   git clone https://github.com/AdeAttwood/NetRpc.git
   cd NetRpc
   ```

2. **Install using Task** (recommended):

   ```bash
   task install
   ```

   This command will:
   - Uninstall any previous version of the CLI tool
   - Build the project in Release mode with version `0.0.0-alpha`
   - Install the CLI globally as a .NET tool

3. **Or install manually**:

   ```bash
   # Uninstall previous version if it exists
   dotnet tool uninstall --global netrpc.cli

   # Build and pack the project
   dotnet pack -c Release /p:Version=0.0.0-alpha

   # Install the tool globally
   dotnet tool install -g --prerelease --add-source ./src/NetRpc.Cli/bin/Release NetRpc.Cli
   ```

### Verify Installation

Check that NetRpc is installed correctly:

```bash
netrpc --version
```

You should see output similar to:

```
NetRpc.Cli 1.0.0
```

### Update from Source

To update to the latest version:

```bash
# Pull latest changes
git pull

# Reinstall
task install
```

### Uninstall

If you need to uninstall:

```bash
dotnet tool uninstall --global netrpc.cli
```

## Install from NuGet (Coming Soon)

Once NetRpc is published to NuGet, you'll be able to install it using:

```bash
dotnet tool install --global NetRpc.Cli
```

And update with:

```bash
dotnet tool update --global NetRpc.Cli
```

## Next Steps

Now that NetRpc is installed, you're ready to:

- [Follow the Quickstart guide](quickstart.md) to build your first API
- [Explore the generators](../generators/overview.md) to understand what NetRpc
  can generate

## Getting Help

If you encounter issues not covered here:

- Check the [GitHub Issues](https://github.com/AdeAttwood/NetRpc/issues)
- Ask in [GitHub Discussions](https://github.com/AdeAttwood/NetRpc/discussions)
- Review the [CLI Reference](../reference/cli.md) for detailed command
  documentation
