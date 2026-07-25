namespace Nop.Plugin.Api.Mobile.Models;

public class ApiResponse<T>
{
    public bool Success { get; set; }

    public T Data { get; set; }

    public ApiError Error { get; set; }

    public static ApiResponse<T> Ok(T data)
    {
        return new ApiResponse<T> { Success = true, Data = data };
    }

    public static ApiResponse<T> Fail(ApiError error)
    {
        return new ApiResponse<T> { Success = false, Error = error };
    }
}

public static class ApiResponse
{
    public static ApiResponse<object> Fail(string code, string message, IDictionary<string, string[]> details = null)
    {
        return ApiResponse<object>.Fail(new ApiError(code, message, details));
    }
}
