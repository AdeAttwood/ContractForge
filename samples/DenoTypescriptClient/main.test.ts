import { assert, assertEquals } from "@std/assert";
import {
  CalculatorClient,
  CalculatorMode,
  RoutedCalculatorClient,
} from "./generated/calculator-service.gen.ts";

let process: Deno.ChildProcess;
const serverPort = 15000 + Math.floor(Math.random() * 10000);
const serverHost = `http://localhost:${serverPort}`;

Deno.test.beforeAll(async () => {
  const server = new Deno.Command("dotnet", {
    args: [
      "run",
      "--project",
      "../DotnetJsonWebApi",
      "--no-launch-profile",
      "--",
      "--urls",
      serverHost,
    ],
    stdout: "piped",
    stderr: "piped",
    env: {
      ASPNETCORE_ENVIRONMENT: "Development",
    },
  });

  process = server.spawn();

  let buffer = "";
  const decoder = new TextDecoder();

  const reader = process.stdout.getReader();
  while (true) {
    const data = await reader.read();
    if (data.done) {
      break;
    }

    buffer += decoder.decode(data.value);

    if (buffer.includes(`Now listening on: ${serverHost}`)) {
      break;
    }
  }
});

Deno.test.afterAll(() => {
  try {
    process?.kill("SIGTERM");
  } catch (_) {
    throw new Error(
      "The server did not exit correctly. It must have exited before the tests have finished",
    );
  }
});

const client = new CalculatorClient({
  host: serverHost,
  resolveHeaders: () => ({
    "X-Client-Token": "sample-token",
  }),
});

const routedClient = new RoutedCalculatorClient({
  host: serverHost,
});

Deno.test("Calls the add method", async () => {
  const result = await client.add({ a: 1, b: 2 });

  assert("type" in result);
  assertEquals("success", result.type);
  if (result.type === "success") {
    assertEquals(3, result.success);
  }
});

Deno.test("Calls the subtract method", async () => {
  const result = await client.subtract({ a: 5, b: 2 });

  assert("type" in result);
  assertEquals("success", result.type);
  if (result.type === "success") {
    assertEquals(3, result.success);
  }
});

Deno.test("Calls add two numbers", async () => {
  const result = await client.addTwoNumbers(1, 1);
  assertEquals(2, result);
});

Deno.test("Server accepts raw default base URL", async () => {
  const response = await fetch(
    `${serverHost}/rpc/calculator-service/add-two-numbers?a=3&b=4`,
    {
      headers: {
        "X-Client-Token": "sample-token",
      },
    },
  );

  assertEquals(200, response.status);
  assertEquals(7, await response.json());
});

Deno.test("Generated client calls normalized custom base URL", async () => {
  const result = await routedClient.addTwoNumbers(3, 4);
  assertEquals(7, result);
});

Deno.test("Server accepts normalized custom base URL", async () => {
  const response = await fetch(
    `${serverHost}/api/calculators/routed-calculator-service/add-two-numbers?a=4&b=5`,
  );

  assertEquals(200, response.status);
  assertEquals(9, await response.json());
});

Deno.test("Calls nested POST params", async () => {
  const result = await client.addNested({ numbers: { a: 4, b: 6 } });
  assertEquals(10, result);
});

Deno.test("Calls nested GET params", async () => {
  const result = await client.addNestedQuery({ numbers: { a: 5, b: 7 } });
  assertEquals(12, result);
});

Deno.test("Adds custom headers via resolveHeaders", async () => {
  const result = await client.addTwoNumbers(2, 3);
  assertEquals(5, result);
});

Deno.test("Returns 0 when client token is missing", async () => {
  const noTokenClient = new CalculatorClient({ host: serverHost });
  const result = await noTokenClient.addTwoNumbers(2, 3);
  assertEquals(0, result);
});

Deno.test("Calls the subtract method with an error", async () => {
  const result = await client.subtract({ a: 0, b: 2 });

  assert("type" in result);
  assertEquals("genericError", result.type);
  if (result.type === "genericError") {
    assertEquals(501, result.genericError.code);
    assertEquals(
      "The service does not handle numbers less than 0",
      result.genericError.message,
    );
  }
});

Deno.test("Calls the subtract method with message-only error", async () => {
  const result = await client.subtract({ a: 5, b: 5 });

  assert("type" in result);
  assertEquals("messageOnlyError", result.type);
  if (result.type === "messageOnlyError") {
    assertEquals(
      "The service does not handle numbers equal to 0",
      result.messageOnlyError.message,
    );
  }
});

Deno.test("Round-trips enum values", async () => {
  const result = await client.echoMode(CalculatorMode.SCIENTIFIC);
  assertEquals(CalculatorMode.SCIENTIFIC, result);
});

Deno.test("Calls the range with await json", async () => {
  const result = await client.range({ a: 5, b: 10 });

  assertEquals("[5,6,7,8,9,10]", JSON.stringify(result));
});

Deno.test("Calls the range with ndjson", async () => {
  const result = [];
  for await (const number of client.rangeStream({ a: 5, b: 10 })) {
    result.push(number);
  }

  assertEquals("[5,6,7,8,9,10]", JSON.stringify(result));
});

Deno.test("Validates union type discrimination", async () => {
  // Test success variant
  const successResult = await client.add({ a: 10, b: 5 });
  assert("type" in successResult);
  assertEquals("success", successResult.type);
  if (successResult.type === "success") {
    assertEquals(15, successResult.success);
  }

  // Test error variant
  const errorResult = await client.subtract({ a: 0, b: 5 });
  assert("type" in errorResult);
  assertEquals("genericError", errorResult.type);
  if (errorResult.type === "genericError") {
    assertEquals(501, errorResult.genericError.code);
  }
});

Deno.test("Handles large streaming dataset", async () => {
  const result = [];
  for await (const number of client.rangeStream({ a: 1, b: 100 })) {
    result.push(number);
  }

  assertEquals(100, result.length);
  assertEquals(1, result[0]);
  assertEquals(100, result[99]);
  // Verify sequence is correct
  for (let i = 0; i < result.length; i++) {
    assertEquals(i + 1, result[i]);
  }
});

Deno.test("Handles network errors gracefully", async () => {
  const badClient = new CalculatorClient({ host: "http://localhost:9999" });

  try {
    await badClient.add({ a: 1, b: 2 });
    assert(false, "Should have thrown an error");
  } catch (error) {
    // Network error should be caught
    assert(error instanceof TypeError || error instanceof Error);
  }
});
