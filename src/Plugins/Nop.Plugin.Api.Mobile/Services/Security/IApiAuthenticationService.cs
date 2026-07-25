using System.Security.Claims;
using Nop.Core.Domain.Customers;

namespace Nop.Plugin.Api.Mobile.Services.Security;

public interface IApiAuthenticationService
{
    Task<LoginResult> LoginAsync(string usernameOrEmail, string password);

    Task LogoutAsync(ClaimsPrincipal userPrincipal);
}

public class LoginResult
{
    public CustomerLoginResults Status { get; set; }

    public string AccessToken { get; set; }

    public int ExpiresInSeconds { get; set; }

    public bool Succeeded => Status == CustomerLoginResults.Successful && !string.IsNullOrEmpty(AccessToken);
}
