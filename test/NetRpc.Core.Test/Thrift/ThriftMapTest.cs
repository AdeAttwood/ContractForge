using NetRpc.Core.Thrift;
using NetRpc.Core.Types;

public class ThriftMapTest
{
    private Document LoadMapInStruct()
    {
        var loader = new ThriftLoader();
        var document = new Document
        {
            Uri = "test.thrift",
            Content = """
            namespace cs Test.ThriftMapTest

            struct UserPreferences {
                map<string, string> settings
            }
            """
        };

        return loader.Load(document);
    }

    [Fact]
    public void Load_WithMapInStruct_HasNoErrors()
    {
        var document = LoadMapInStruct();
        Assert.Empty(document.Errors);
    }

    [Fact]
    public void Load_WithMapInStruct_HasOneStruct()
    {
        var document = LoadMapInStruct();
        Assert.Single(document.Structs);
    }

    [Fact]
    public void Load_WithMapInStruct_FieldIsMapType()
    {
        var document = LoadMapInStruct();
        var structType = document.Structs["UserPreferences"];
        var field = structType.Fields["settings"];
        Assert.IsType<Map>(field.Type);
    }

    [Fact]
    public void Load_WithMapInStruct_MapHasCorrectKeyType()
    {
        var document = LoadMapInStruct();
        var structType = document.Structs["UserPreferences"];
        var field = structType.Fields["settings"];
        var mapType = (Map)field.Type;
        Assert.IsType<Primitive>(mapType.Key);
        Assert.Equal("string", ((Primitive)mapType.Key).Type);
    }

    [Fact]
    public void Load_WithMapInStruct_MapHasCorrectValueType()
    {
        var document = LoadMapInStruct();
        var structType = document.Structs["UserPreferences"];
        var field = structType.Fields["settings"];
        var mapType = (Map)field.Type;
        Assert.IsType<Primitive>(mapType.Value);
        Assert.Equal("string", ((Primitive)mapType.Value).Type);
    }

    private Document LoadMapInServiceFunction()
    {
        var loader = new ThriftLoader();
        var document = new Document
        {
            Uri = "test.thrift",
            Content = """
            namespace cs Test.ThriftMapTest

            struct User {
                i32 id,
                string name
            }

            service UserService {
                map<i32, User> getUserMap(list<i32> ids)
            }
            """
        };

        return loader.Load(document);
    }

    [Fact]
    public void Load_WithMapReturnType_HasNoErrors()
    {
        var document = LoadMapInServiceFunction();
        Assert.Empty(document.Errors);
    }

    [Fact]
    public void Load_WithMapReturnType_FunctionReturnsMap()
    {
        var document = LoadMapInServiceFunction();
        var service = document.Services["UserService"];
        var function = service.Functions["getUserMap"];
        Assert.IsType<Map>(function.Type);
    }

    [Fact]
    public void Load_WithMapReturnType_MapHasCorrectTypes()
    {
        var document = LoadMapInServiceFunction();
        var service = document.Services["UserService"];
        var function = service.Functions["getUserMap"];
        var mapType = (Map)function.Type;

        Assert.IsType<Primitive>(mapType.Key);
        Assert.Equal("i32", ((Primitive)mapType.Key).Type);

        Assert.IsType<Struct>(mapType.Value);
        Assert.Equal("User", ((Struct)mapType.Value).Identifier);
    }

    private Document LoadNestedMap()
    {
        var loader = new ThriftLoader();
        var document = new Document
        {
            Uri = "test.thrift",
            Content = """
            namespace cs Test.ThriftMapTest

            struct ComplexData {
                map<string, list<i32>> dataMap
            }
            """
        };

        return loader.Load(document);
    }

    [Fact]
    public void Load_WithNestedMapAndList_HasNoErrors()
    {
        var document = LoadNestedMap();
        Assert.Empty(document.Errors);
    }

    [Fact]
    public void Load_WithNestedMapAndList_MapValueIsList()
    {
        var document = LoadNestedMap();
        var structType = document.Structs["ComplexData"];
        var field = structType.Fields["dataMap"];
        var mapType = (Map)field.Type;

        Assert.IsType<List>(mapType.Value);
        var listType = (List)mapType.Value;
        Assert.IsType<Primitive>(listType.InnerType);
        Assert.Equal("i32", ((Primitive)listType.InnerType).Type);
    }
}