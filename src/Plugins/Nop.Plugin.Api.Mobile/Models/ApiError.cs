namespace Nop.Plugin.Api.Mobile.Models;

/// <summary>
/// Represents an error payload returned by the API
/// </summary>
public class ApiError
{
    /// <summary>
    /// Gets or sets a machine-readable error code
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// Gets or sets a human-readable error message
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Gets or sets optional field-level validation errors (field name => messages)
    /// </summary>
    public IDictionary<string, string[]> Details { get; set; }

    public ApiError()
    {
    }

    public ApiError(string code, string message, IDictionary<string, string[]> details = null)
    {
        Code = code;
        Message = message;
        Details = details;
    }
}
