using System.Security.Claims;
using Nop.Core.Domain.Customers;
using Nop.Services.Customers;

namespace Nop.Plugin.Api.Mobile.Services.Security;

public class ApiAuthenticationService : IApiAuthenticationService
{
    #region Fields

    protected readonly ICustomerRegistrationService _customerRegistrationService;
    protected readonly ICustomerService _customerService;
    protected readonly ITokenService _tokenService;
    protected readonly IBlacklistService _blacklistService;
    protected readonly CustomerSettings _customerSettings;

    #endregion

    #region Ctor

    public ApiAuthenticationService(ICustomerRegistrationService customerRegistrationService,
        ICustomerService customerService,
        ITokenService tokenService,
        IBlacklistService blacklistService,
        CustomerSettings customerSettings)
    {
        _customerRegistrationService = customerRegistrationService;
        _customerService = customerService;
        _tokenService = tokenService;
        _blacklistService = blacklistService;
        _customerSettings = customerSettings;
    }

    #endregion

    #region Methods

    public async Task<LoginResult> LoginAsync(string usernameOrEmail, string password)
    {
        var status = await _customerRegistrationService.ValidateCustomerAsync(usernameOrEmail, password);
        if (status != CustomerLoginResults.Successful)
            return new LoginResult { Status = status };

        var customer = _customerSettings.UsernamesEnabled
            ? await _customerService.GetCustomerByUsernameAsync(usernameOrEmail)
            : await _customerService.GetCustomerByEmailAsync(usernameOrEmail);

        if (customer == null)
            return new LoginResult { Status = CustomerLoginResults.CustomerNotExist };

        return new LoginResult
        {
            Status = CustomerLoginResults.Successful,
            AccessToken = _tokenService.GenerateAccessToken(customer),
            ExpiresInSeconds = _tokenService.AccessTokenExpirationSeconds
        };
    }

    public Task LogoutAsync(ClaimsPrincipal userPrincipal)
    {
        return _blacklistService.BlacklistTokenAsync(userPrincipal);
    }

    #endregion
}
