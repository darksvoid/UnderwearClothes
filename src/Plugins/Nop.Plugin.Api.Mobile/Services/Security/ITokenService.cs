using Nop.Core.Domain.Customers;

namespace Nop.Plugin.Api.Mobile.Services.Security;

/// <summary>
/// Issues JWT access tokens for authenticated customers.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates a signed JWT access token for the given customer.
    /// </summary>
    string GenerateAccessToken(Customer customer);

    /// <summary>
    /// Gets the configured access token lifetime, in seconds.
    /// </summary>
    int AccessTokenExpirationSeconds { get; }
}
