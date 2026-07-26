using System.Security.Claims;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Api.Mobile.Models.Auth;

namespace Nop.Plugin.Api.Mobile.Services.Security;

public interface IApiAuthenticationService
{
    Task<LoginResult> LoginAsync(string usernameOrEmail, string password);

    Task<RegisterResult> RegisterAsync(RegisterRequest request);

    Task LogoutAsync(ClaimsPrincipal userPrincipal);
}

public class LoginResult
{
    public CustomerLoginResults Status { get; set; }

    public string AccessToken { get; set; }

    public int ExpiresInSeconds { get; set; }

    public bool Succeeded => Status == CustomerLoginResults.Successful && !string.IsNullOrEmpty(AccessToken);
}

public class RegisterResult
{
    public bool Succeeded { get; set; }

    public bool RegistrationDisabled { get; set; }

    public bool RequiresEmailValidation { get; set; }

    public bool RequiresApproval { get; set; }

    public string AccessToken { get; set; }

    public int ExpiresInSeconds { get; set; }

    public IList<string> Errors { get; set; } = new List<string>();
}
