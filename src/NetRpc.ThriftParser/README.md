```
curl -O https://www.antlr.org/download/antlr-4.13.2-complete.jar
java -jar ./artifacts/antlr-4.13.2-complete.jar -Dlanguage=CSharp -package NetRpc.ThriftParser  -visitor -listener ./src/NetRpc.ThriftParser/Thrift.g4 -o ./src/NetRpc.ThriftParser
```

```
docker run -w /src -v $"(pwd):/src" -ti --network bridge --rm opensuse/tumbleweed bash
zypper install antlr4-tool
```

```
java -jar ./antlr-4.13.2-complete.jar -Dlanguage=CSharp -package NetRpc.ThriftParser  -visitor -listener ./Thrift.g4
```
