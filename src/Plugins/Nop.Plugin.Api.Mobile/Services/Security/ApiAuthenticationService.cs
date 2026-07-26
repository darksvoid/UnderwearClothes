using System.Security.Claims;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Api.Mobile.Models.Auth;
using Nop.Services.Customers;

namespace Nop.Plugin.Api.Mobile.Services.Security;

public class ApiAuthenticationService : IApiAuthenticationService
{
    #region Fields

    protected readonly IStoreContext _storeContext;
    protected readonly ICustomerRegistrationService _customerRegistrationService;
    protected readonly ICustomerService _customerService;
    protected readonly ITokenService _tokenService;
    protected readonly IBlacklistService _blacklistService;
    protected readonly CustomerSettings _customerSettings;

    #endregion

    #region Ctor

    public ApiAuthenticationService(IStoreContext storeContext,
        ICustomerRegistrationService customerRegistrationService,
        ICustomerService customerService,
        ITokenService tokenService,
        IBlacklistService blacklistService,
        CustomerSettings customerSettings)
    {
        _storeContext = storeContext;
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

    public async Task<RegisterResult> RegisterAsync(RegisterRequest request)
    {
        if (_customerSettings.UserRegistrationType == UserRegistrationType.Disabled)
            return new RegisterResult { RegistrationDisabled = true };

        //always register a fresh guest — the API is stateless and must not depend on the ambient
        //(cookie-based) work context customer, which may already be registered
        var customer = await _customerService.InsertGuestCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();

        var username = _customerSettings.UsernamesEnabled ? request.Username : request.Email;
        var isApproved = _customerSettings.UserRegistrationType == UserRegistrationType.Standard;

        var registrationRequest = new CustomerRegistrationRequest(
            customer,
            request.Email,
            username,
            request.Password,
            _customerSettings.DefaultPasswordFormat,
            store.Id,
            isApproved);

        var result = await _customerRegistrationService.RegisterCustomerAsync(registrationRequest);
        if (!result.Success)
            return new RegisterResult { Errors = result.Errors };

        if (!string.IsNullOrWhiteSpace(request.FirstName))
            customer.FirstName = request.FirstName;
        if (!string.IsNullOrWhiteSpace(request.LastName))
            customer.LastName = request.LastName;
        await _customerService.UpdateCustomerAsync(customer);

        if (_customerSettings.UserRegistrationType == UserRegistrationType.Standard)
            return new RegisterResult
            {
                Succeeded = true,
                AccessToken = _tokenService.GenerateAccessToken(customer),
                ExpiresInSeconds = _tokenService.AccessTokenExpirationSeconds
            };

        return new RegisterResult
        {
            Succeeded = true,
            RequiresEmailValidation = _customerSettings.UserRegistrationType == UserRegistrationType.EmailValidation,
            RequiresApproval = _customerSettings.UserRegistrationType == UserRegistrationType.AdminApproval
        };
    }

    public Task LogoutAsync(ClaimsPrincipal userPrincipal)
    {
        return _blacklistService.BlacklistTokenAsync(userPrincipal);
    }

    #endregion
}
