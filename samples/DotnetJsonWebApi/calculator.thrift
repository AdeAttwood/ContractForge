namespace cs DotnetJsonWebApi.Generated

exception GenericError {
  required i32 code,
  required string message
}

union NumberResult {
  i64 success,
  GenericError genericError
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
   * Subtract two numbers. The result of `a - b` will be returned.
   */
  NumberResult subtract(NumberRequestParams requestParams) throws (GenericError genericError),
  /**
   * Get all the number between `a` and `b`
   */
   list<i64> range(NumberRequestParams requestParams)
}
