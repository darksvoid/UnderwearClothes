using Nop.Core.Domain.Customers;

namespace Nop.Plugin.Api.Mobile.Services.Security;

public interface ITokenService
{
    string GenerateAccessToken(Customer customer);

    int AccessTokenExpirationSeconds { get; }
}
