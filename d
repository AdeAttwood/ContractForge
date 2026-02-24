Add partial class generator option for C# output

Summary:
Users can now request partial C# DTOs from the CLI, so generated
DTO, union, and exception types can be extended safely without editing
generator output.
To pass options, parser logic now validates key-value options through
an explicit C# options type. Unknown keys and invalid boolean values now
fail early with actionable errors instead of being silently ignored.

Test Plan:
1. dotnet test test/ContractForge.Core.Test/ContractForge.Core.Test.csproj --filter "FullyQualifiedName~CSharpCodeGenOptionsTest"
2. Confirm the targeted tests pass and include partial-generation and
   unsupported-alias coverage.
3. Expected result: C# types are generated as partial classes only when
   partial=true is provided.
