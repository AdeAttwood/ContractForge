#!/usr/bin/env bash

set -euo pipefail

package_source=$(realpath "$1")
package_version=$2
expected_runtime_major=$3
fixture=$(realpath "$4")
work_dir="${RUNNER_TEMP:-/tmp}/contractforge-package-test"
tool_dir="$work_dir/tool"
nuget_config="$work_dir/NuGet.Config"

rm -rf "$work_dir"
mkdir -p "$tool_dir"
cd "$work_dir"

printf '%s\n' \
  '<?xml version="1.0" encoding="utf-8"?>' \
  '<configuration>' \
  '  <packageSources>' \
  '    <clear />' \
  "    <add key=\"local-package\" value=\"$package_source\" />" \
  '  </packageSources>' \
  '</configuration>' > "$nuget_config"

runtime_majors=$(dotnet --list-runtimes \
  | awk '$1 == "Microsoft.NETCore.App" { split($2, version, "."); print version[1] }' \
  | sort -u)
test "$runtime_majors" = "$expected_runtime_major"

dotnet tool install \
  --tool-path "$tool_dir" \
  --configfile "$nuget_config" \
  --version "$package_version" \
  ContractForge.Cli

COREHOST_TRACE=1 \
  COREHOST_TRACE_VERBOSITY=4 \
  COREHOST_TRACEFILE="$work_dir/host.trace" \
  "$tool_dir/contractforge" --version
grep -Fq "/shared/Microsoft.NETCore.App/$expected_runtime_major." "$work_dir/host.trace"

"$tool_dir/contractforge" --help | grep -Fq "USAGE:"
"$tool_dir/contractforge" lint --entry "$fixture" | grep -Fq "No issues found."
"$tool_dir/contractforge" \
  --entry "$fixture" \
  --generator csharp-jsonapi \
  --output "$work_dir/generated.cs"
grep -Fq "CalculatorService" "$work_dir/generated.cs"

initialize='{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"processId":null,"rootUri":null,"capabilities":{}}}'
initialized='{"jsonrpc":"2.0","method":"initialized","params":{}}'
shutdown='{"jsonrpc":"2.0","id":2,"method":"shutdown","params":null}'
exit_notification='{"jsonrpc":"2.0","method":"exit","params":null}'

{
  printf 'Content-Length: %s\r\n\r\n%s' "${#initialize}" "$initialize"
  printf 'Content-Length: %s\r\n\r\n%s' "${#initialized}" "$initialized"
  printf 'Content-Length: %s\r\n\r\n%s' "${#shutdown}" "$shutdown"
  printf 'Content-Length: %s\r\n\r\n%s' "${#exit_notification}" "$exit_notification"
} | timeout 20s "$tool_dir/contractforge" lsp > "$work_dir/lsp.out"

grep -Fq '"id":1' "$work_dir/lsp.out"
grep -Fq '"id":2' "$work_dir/lsp.out"
