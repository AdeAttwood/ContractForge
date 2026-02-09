using NetRpc.Core.CSharp;
using NetRpc.Core.Thrift;
using NetRpc.Core.Types;
using NetRpc.Core.Typescript;

namespace NetRpc.Core.Test.TestHelpers;

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
        return loader.Load(document);
    }

    protected CodeGenResult GenerateCSharp(string thriftContent)
    {
        var document = LoadThrift(thriftContent);
        var state = new DefinitionState();
        state.Documents.Add("test.thrift", document);
        var generator = new CSharpCodeGen();
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