namespace Nop.Plugin.Api.Mobile.Models;

/// <summary>
/// Represents a uniform envelope for every API response
/// </summary>
/// <typeparam name="T">Type of the payload</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Gets or sets a value indicating whether the request succeeded
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the payload (null on error)
    /// </summary>
    public T Data { get; set; }

    /// <summary>
    /// Gets or sets the error information (null on success)
    /// </summary>
    public ApiError Error { get; set; }

    /// <summary>
    /// Builds a successful response with the given payload
    /// </summary>
    public static ApiResponse<T> Ok(T data)
    {
        return new ApiResponse<T> { Success = true, Data = data };
    }

    /// <summary>
    /// Builds a failed response with the given error
    /// </summary>
    public static ApiResponse<T> Fail(ApiError error)
    {
        return new ApiResponse<T> { Success = false, Error = error };
    }
}

/// <summary>
/// Non-generic helpers for building error envelopes when there is no payload type in hand
/// </summary>
public static class ApiResponse
{
    public static ApiResponse<object> Fail(string code, string message, IDictionary<string, string[]> details = null)
    {
        return ApiResponse<object>.Fail(new ApiError(code, message, details));
    }
}
