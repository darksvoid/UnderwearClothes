namespace Nop.Plugin.Api.Mobile.Models;

public class ApiError
{
    public string Code { get; set; }

    public string Message { get; set; }

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
