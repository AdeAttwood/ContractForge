using System.Text;

using ContractForge.Core.Types;

namespace ContractForge.Core.Typescript.Generators;

/// <summary>
/// Generates TypeScript client classes from service definitions
/// </summary>
public class TypeScriptClientGenerator : ITypeScriptGenerator
{
    private readonly TypeScriptTypeMapper _typeMapper;

    public TypeScriptClientGenerator(TypeScriptTypeMapper typeMapper)
    {
        _typeMapper = typeMapper;
    }

    public void Generate(StringBuilder builder, Document document, List<Error> errors)
    {
        foreach (var service in document.Services.Values)
        {
            builder.AppendFormat("\n", service.Identifier);
            builder.AppendFormat(
                """
                export interface {0}ClientOptions {{
                  host: string;
                  resolveHeaders?: (request: {{ path: string; method: 'GET' | 'POST' }}) => Promise<Record<string, string>> | Record<string, string>;
                }}

                """,
                  service.Identifier
            );

            TypeScriptDocumentationHelper.AppendJsDocComment(builder, service.Description, 0);
            builder.AppendFormat(
                """
                export class {0}Client {{
                  public constructor(private options: {0}ClientOptions) {{}}

                  private buildUrl(path: string, query: Record<string, unknown>) {{
                    const url = new URL(this.options.host);
                    url.pathname = path;

                    const flattened = flatten(query);
                    for (const key in flattened) {{
                      if (flattened[key]) {{
                        url.searchParams.set(key, String(flattened[key]))
                      }}
                    }}

                    return url;
                  }}

                  private serialize(obj: unknown) {{
                      return JSON.stringify(obj);
                  }}

                  private async buildHeaders(
                    path: string,
                    method: 'GET' | 'POST'
                  ): Promise<Record<string, string>> {{
                    const resolvedHeaders = await this.options.resolveHeaders?.({{ path, method }}) ?? {{}};

                    return {{
                      ...resolvedHeaders,
                      "Content-Type": "application/json",
                    }};
                  }}

                  private async request(
                    path: string,
                    method: 'GET' | 'POST',
                    params: unknown
                  ) {{
                    const url = method === 'GET'
                      ? this.buildUrl(path, params as Record<string, unknown>)
                      : this.buildUrl(path, {{}});
                    const headers = {{
                      ...(await this.buildHeaders(path, method)),
                      "Accept": "application/json",
                    }};

                    return await fetch(url.toString(), {{
                      method,
                      body: method === 'POST' ? this.serialize(params) : undefined,
                      headers,
                    }})
                    .then(r => r.json())
                    .catch(e => e);
                  }}

                  private async *streamNdjson<T>(
                    path: string,
                    method: 'GET' | 'POST',
                    params: unknown
                  ): AsyncIterable<T> {{
                    const url = method === 'GET'
                      ? this.buildUrl(path, params as Record<string, unknown>)
                      : this.buildUrl(path, {{}});
                    const headers = {{
                      ...(await this.buildHeaders(path, method)),
                      "Accept": "application/x-ndjson",
                    }};

                    const response = await fetch(url.toString(), {{
                      method,
                      body: method === 'POST' ? this.serialize(params) : undefined,
                      headers,
                    }});

                    if (!response.ok) {{
                      throw new Error(`HTTP error! status: ${{response.status}}`);
                    }}

                    const reader = response.body!.getReader();
                    const decoder = new TextDecoder();
                    let buffer = '';

                    while (true) {{
                      const {{ done, value }} = await reader.read();
                      if (done) break;

                      buffer += decoder.decode(value, {{ stream: true }});
                      const lines = buffer.split('\n');
                      buffer = lines.pop() || '';

                      for (const line of lines) {{
                        if (line.trim()) {{
                          yield JSON.parse(line) as T;
                        }}
                      }}
                    }}

                    if (buffer.trim()) {{
                      yield JSON.parse(buffer) as T;
                    }}
                  }}

                """,
                  service.Identifier
            );

            foreach (var func in service.Functions.Values)
            {
                TypeScriptDocumentationHelper.AppendJsDocComment(builder, func.Description, 2);
                builder.AppendFormat("  public async {0}(", func.Identifier);

                var parameters = func.Parameters.Values.Select((parameter) =>
                {
                    return $"{parameter.Identifier}: {_typeMapper.ToTypeScriptType(parameter.Type)}";
                });

                var returnType = string.Join(" | ", func.AllTypes().Select(_typeMapper.ToTypeScriptType));

                builder.AppendJoin(", ", parameters);
                builder.AppendFormat("): Promise<{0}> {{\n", returnType);

                // Build the params argument for the request
                var paramNames = func.Parameters.Values.Select(p => p.Identifier).ToList();
                var paramsArg = paramNames.Count == 0
                    ? "{}"
                    : paramNames.Count == 1
                        ? paramNames[0]  // Single param: pass directly
                        : $"{{ {string.Join(", ", paramNames)} }}";  // Multiple params: wrap in object

                builder.AppendFormat(
                    """
                        return await this.request(
                          "{0}",
                          "{1}",
                          {2}
                        );

                    """,
                    $"{service.Url()}/{func.Url()}",
                    func.Method().ToUpper(),
                    paramsArg
                );

                builder.Append("  }\n");

                // If the function returns a List type, generate a streaming version
                if (func.Type is List listType)
                {
                    var innerType = _typeMapper.ToTypeScriptType(listType.InnerType);
                    var parametersStr = string.Join(", ", parameters);
                    var streamParamNames = string.Join(", ", func.Parameters.Values.Select(p => p.Identifier));


                    TypeScriptDocumentationHelper.AppendJsDocComment(builder, func.Description, 2);
                    builder.AppendFormat("  public async *{0}Stream(", func.Identifier);
                    builder.Append(parametersStr);
                    builder.AppendFormat("): AsyncIterable<{0}> {{\n", innerType);
                    builder.AppendFormat(
                        """
                            yield* this.streamNdjson<{0}>(
                              "{1}",
                              "{2}",
                              {3}
                            );

                        """,
                        innerType,
                        $"{service.Url()}/{func.Url()}",
                        func.Method().ToUpper(),
                        streamParamNames.Length > 0 ? streamParamNames : "{}"
                    );

                    builder.Append("  }\n");
                }
            }

            builder.Append("}\n");
        }
    }
}