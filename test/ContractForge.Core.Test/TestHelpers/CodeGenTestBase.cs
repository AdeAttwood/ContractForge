using ContractForge.Core.CSharp;
using ContractForge.Core.Thrift;
using ContractForge.Core.Types;
using ContractForge.Core.Typescript;

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
}