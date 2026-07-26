using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Api.Mobile.Models;
using Nop.Plugin.Api.Mobile.Models.Auth;
using Nop.Plugin.Api.Mobile.Services.Security;

namespace Nop.Plugin.Api.Mobile.Controllers;

/// <summary>
/// Authentication endpoints: obtaining and revoking access tokens.
/// </summary>
public class AuthController : BaseApiController
{
    #region Fields

    protected readonly IApiAuthenticationService _authenticationService;

    #endregion

    #region Ctor

    public AuthController(IApiAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Authenticates a customer and returns a JWT access token.
    /// </summary>
    /// <response code="200">Authentication succeeded; the access token is returned.</response>
    /// <response code="401">Invalid credentials or the account cannot be logged in.</response>
    [HttpPost("token")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<TokenResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Token([FromBody] LoginRequest request)
    {
        var result = await _authenticationService.LoginAsync(request.UsernameOrEmail, request.Password);

        if (!result.Succeeded)
            return Unauthorized(ApiResponse.Fail("authentication_failed", MapStatusMessage(result.Status)));

        return Success(new TokenResponse
        {
            AccessToken = result.AccessToken,
            ExpiresIn = result.ExpiresInSeconds
        });
    }

    /// <summary>
    /// Registers a new customer. Returns an access token when the store allows immediate login
    /// (standard registration), otherwise indicates that email confirmation or admin approval is required.
    /// </summary>
    /// <response code="200">Registration succeeded.</response>
    /// <response code="400">Registration failed or is disabled.</response>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<RegisterResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authenticationService.RegisterAsync(request);

        if (result.RegistrationDisabled)
            return BadRequest(ApiResponse.Fail("registration_disabled", "Registration is disabled."));

        if (!result.Succeeded)
        {
            var details = new Dictionary<string, string[]> { ["errors"] = result.Errors.ToArray() };
            return BadRequest(ApiResponse.Fail("registration_failed", string.Join("; ", result.Errors), details));
        }

        if (!string.IsNullOrEmpty(result.AccessToken))
            return Success(new RegisterResponse
            {
                AccessToken = result.AccessToken,
                TokenType = "Bearer",
                ExpiresIn = result.ExpiresInSeconds
            });

        return Success(new RegisterResponse
        {
            RequiresEmailValidation = result.RequiresEmailValidation,
            RequiresApproval = result.RequiresApproval
        });
    }

    /// <summary>
    /// Revokes the current access token. The token cannot be used afterwards.
    /// </summary>
    /// <response code="200">The token has been revoked.</response>
    /// <response code="401">No valid token was presented.</response>
    [HttpPost("logout")]
    [Authorize(AuthenticationSchemes = ApiMobileDefaults.AuthenticationScheme)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout()
    {
        await _authenticationService.LogoutAsync(User);
        return Success(new { message = "The token has been revoked." });
    }

    #endregion

    #region Utilities

    private static string MapStatusMessage(CustomerLoginResults status)
    {
        return status switch
        {
            CustomerLoginResults.CustomerNotExist => "Customer does not exist.",
            CustomerLoginResults.WrongPassword => "The password is incorrect.",
            CustomerLoginResults.NotActive => "The account is not active.",
            CustomerLoginResults.Deleted => "The account has been deleted.",
            CustomerLoginResults.NotRegistered => "The account is not registered.",
            CustomerLoginResults.LockedOut => "The account is locked out. Try again later.",
            CustomerLoginResults.MultiFactorAuthenticationRequired => "Multi-factor authentication is not supported by the mobile API.",
            _ => "Invalid username or password."
        };
    }

    #endregion
}
