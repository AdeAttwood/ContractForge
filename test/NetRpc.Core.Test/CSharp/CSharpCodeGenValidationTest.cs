using NetRpc.Core.CSharp;
using NetRpc.Core.Thrift;
using NetRpc.Core.Types;

namespace NetRpc.Core.Test.CSharp;

public class CSharpCodeGenValidationTest
{
    [Fact]
    public Task Generate_RequiredFields_ProducesRequiredAttributeAndNonNullable()
    {
        var document = LoadThrift(@"
            namespace cs Test

            struct User {
                required i32 id,
                required string name,
                required bool isActive
            }
        ");

        Assert.Empty(document.Errors);

        var state = new DefinitionState();
        state.Documents.Add("test.thrift", document);

        var generator = new CSharpCodeGen();
        var result = generator.Build(state);

        return Verify(result.Output).UseDirectory("Snapshots");
    }

    [Fact]
    public Task Generate_OptionalFields_ProducesNullableWithoutRequiredAttribute()
    {
        var document = LoadThrift(@"
            namespace cs Test

            struct User {
                optional i32 age,
                optional string email,
                optional bool isVerified
            }
        ");

        Assert.Empty(document.Errors);

        var state = new DefinitionState();
        state.Documents.Add("test.thrift", document);

        var generator = new CSharpCodeGen();
        var result = generator.Build(state);

        return Verify(result.Output).UseDirectory("Snapshots");
    }

    [Fact]
    public Task Generate_DefaultFields_ProducesNullableWithoutRequiredAttribute()
    {
        var document = LoadThrift(@"
            namespace cs Test

            struct User {
                i32 score,
                string nickname,
                bool isPremium
            }
        ");

        Assert.Empty(document.Errors);

        var state = new DefinitionState();
        state.Documents.Add("test.thrift", document);

        var generator = new CSharpCodeGen();
        var result = generator.Build(state);

        return Verify(result.Output).UseDirectory("Snapshots");
    }

    [Fact]
    public Task Generate_MixedRequiredOptionalFields_ProducesCorrectAttributes()
    {
        var document = LoadThrift(@"
            namespace cs Test

            struct User {
                required i32 id,
                required string name,
                optional string email,
                optional i32 age,
                bool isActive
            }
        ");

        Assert.Empty(document.Errors);

        var state = new DefinitionState();
        state.Documents.Add("test.thrift", document);

        var generator = new CSharpCodeGen();
        var result = generator.Build(state);

        return Verify(result.Output).UseDirectory("Snapshots");
    }

    [Fact]
    public Task Generate_RequiredComplexTypes_ProducesRequiredAttributeAndNonNullable()
    {
        var document = LoadThrift(@"
            namespace cs Test

            struct Address {
                required string street
            }

            struct User {
                required Address address,
                required list<string> tags,
                required map<string, string> metadata
            }
        ");

        Assert.Empty(document.Errors);

        var state = new DefinitionState();
        state.Documents.Add("test.thrift", document);

        var generator = new CSharpCodeGen();
        var result = generator.Build(state);

        return Verify(result.Output).UseDirectory("Snapshots");
    }

    [Fact]
    public Task Generate_OptionalComplexTypes_ProducesNullableWithoutRequiredAttribute()
    {
        var document = LoadThrift(@"
            namespace cs Test

            struct Address {
                optional string street
            }

            struct User {
                optional Address address,
                optional list<string> tags,
                optional map<string, string> metadata
            }
        ");

        Assert.Empty(document.Errors);

        var state = new DefinitionState();
        state.Documents.Add("test.thrift", document);

        var generator = new CSharpCodeGen();
        var result = generator.Build(state);

        return Verify(result.Output).UseDirectory("Snapshots");
    }

    [Fact]
    public Task Generate_ExceptionWithRequiredFields_ProducesRequiredAttribute()
    {
        var document = LoadThrift(@"
            namespace cs Test

            exception ValidationError {
                required i32 code,
                required string message,
                optional string field
            }
        ");

        Assert.Empty(document.Errors);

        var state = new DefinitionState();
        state.Documents.Add("test.thrift", document);

        var generator = new CSharpCodeGen();
        var result = generator.Build(state);

        return Verify(result.Output).UseDirectory("Snapshots");
    }

    private Document LoadThrift(string content)
    {
        var loader = new ThriftLoader();
        var document = new Document
        {
            Uri = "test.thrift",
            Content = content
        };

        loader.Load(document, new DefinitionState());

        return document;
    }
}