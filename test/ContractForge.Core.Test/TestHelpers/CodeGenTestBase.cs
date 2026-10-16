using System.Reflection;
using System.Runtime.Loader;

using ContractForge.Core.CSharp;
using ContractForge.Core.OpenApi;
using ContractForge.Core.Thrift;
using ContractForge.Core.Types;
using ContractForge.Core.Typescript;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ContractForge.Core.Test.TestHelpers;

public abstract class CodeGenTestBase
{
    protected Document LoadThrift(string content)
    {
        var loader = new ThriftLoader();
        var document = new Document
        {
            Uri = "test.thrift",
            Content = content
        };
        return loader.Load(document, new DefinitionState());
    }

    protected CodeGenResult GenerateCSharp(string thriftContent, CSharpCodeGenOptions? options = null)
    {
        var document = LoadThrift(thriftContent);
        var state = new DefinitionState();
        state.Documents.Add("test.thrift", document);
        var generator = new CSharpCodeGen(options);
        return generator.Build(state);
    }

    protected CodeGenResult GenerateTypeScript(string thriftContent)
    {
        var document = LoadThrift(thriftContent);
        var state = new DefinitionState();
        state.Documents.Add("test.thrift", document);
        var generator = new TypescriptCodeGen();
        return generator.Build(state);
    }

    protected CodeGenResult GenerateOpenApi(string thriftContent, OpenApiCodeGenOptions? options = null)
    {
        var document = LoadThrift(thriftContent);
        var state = new DefinitionState();
        state.Documents.Add("test.thrift", document);
        var generator = new OpenApiCodeGen(options);
        return generator.Build(state);
    }

    protected static async Task CompileAndRun(string generatedSource, string testSource)
    {
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path));
        var compilation = CSharpCompilation.Create(
            $"GeneratedUnionTests_{Guid.NewGuid():N}",
            [
                CSharpSyntaxTree.ParseText(generatedSource),
                CSharpSyntaxTree.ParseText(testSource)
            ],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        using var assemblyStream = new MemoryStream();
        var emitResult = compilation.Emit(assemblyStream);
        Assert.True(
            emitResult.Success,
            string.Join(Environment.NewLine, emitResult.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)));

        assemblyStream.Position = 0;
        var loadContext = new AssemblyLoadContext(compilation.AssemblyName!, isCollectible: true);
        try
        {
            var assembly = loadContext.LoadFromStream(assemblyStream);
            var runMethod = assembly.GetType("RuntimeTests.GeneratedUnionTests")!
                .GetMethod("Run", BindingFlags.Public | BindingFlags.Static)!;
            await (Task)runMethod.Invoke(null, null)!;
        }
        finally
        {
            loadContext.Unload();
        }
    }

}