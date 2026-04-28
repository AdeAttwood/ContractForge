namespace cs DotnetJsonWebApi.Generated

exception GenericError {
  required i32 code,
  required string message
}

exception MessageOnlyError {
  required string message
}

union NumberResult {
  i64 success,
  GenericError genericError,
  MessageOnlyError messageOnlyError
}

enum CalculatorMode {
  Standard = 0,
  Scientific = 1
}

/**
 * The parameters to send to some of the calculator service methods.
 */
struct NumberRequestParams {
  /**
   * The first number to use
   */
  required i64 a,
  /**
   * The second number to use
   */
  required i64 b
}

/**
 * Wraps number parameters to verify nested GET query flattening and POST bodies.
 */
struct NestedNumberRequestParams {
  required NumberRequestParams numbers
}

/**
 * Calculator service
 */
service Calculator {
  /**
   * Adds two numbers together. The result of `a + b` will be returned.
   */
  NumberResult add(NumberRequestParams requestParams) throws (GenericError genericError),
  /**
   * Add two numbers that are bound to multiple params
   */
  i32 addTwoNumbers(i32 a, i32 b) (http.method = "get"),
  /**
   * Add nested request parameters using a POST body.
   */
  i64 addNested(NestedNumberRequestParams requestParams),
  /**
   * Add nested request parameters using flattened GET query parameters.
   */
  i64 addNestedQuery(NestedNumberRequestParams requestParams) (http.method = "get"),
  /**
   * Echo the calculator mode for enum round-trip testing.
   */
  CalculatorMode echoMode(CalculatorMode mode),
  /**
   * Subtract two numbers. The result of `a - b` will be returned.
   */
  NumberResult subtract(NumberRequestParams requestParams) throws (GenericError genericError),
  /**
   * Get all the number between `a` and `b`
   */
   list<i64> range(NumberRequestParams requestParams)
}

/**
 * Calculator service mounted under a custom base URL.
 */
service RoutedCalculator {
  /**
   * Add two numbers through a custom base URL.
   */
  i32 addTwoNumbers(i32 a, i32 b) (http.method = "get")
} (http.baseUrl = "/api/calculators")
