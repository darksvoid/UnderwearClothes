using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Api.Mobile.Factories;
using Nop.Plugin.Api.Mobile.Models;
using Nop.Plugin.Api.Mobile.Models.Account;
using Nop.Plugin.Api.Mobile.Models.Customers;
using Nop.Services.Customers;

namespace Nop.Plugin.Api.Mobile.Controllers;

/// <summary>
/// Authenticated account management: profile update and password change.
/// </summary>
[Authorize(AuthenticationSchemes = ApiMobileDefaults.AuthenticationScheme)]
public class AccountController : BaseApiController
{
    #region Fields

    protected readonly IWorkContext _workContext;
    protected readonly ICustomerService _customerService;
    protected readonly ICustomerRegistrationService _customerRegistrationService;
    protected readonly ICustomerModelFactory _customerModelFactory;

    #endregion

    #region Ctor

    public AccountController(IWorkContext workContext,
        ICustomerService customerService,
        ICustomerRegistrationService customerRegistrationService,
        ICustomerModelFactory customerModelFactory)
    {
        _workContext = workContext;
        _customerService = customerService;
        _customerRegistrationService = customerRegistrationService;
        _customerModelFactory = customerModelFactory;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Updates the profile fields of the authenticated customer.
    /// </summary>
    [HttpPut("profile")]
    [ProducesResponseType(typeof(ApiResponse<CustomerProfileModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();

        customer.FirstName = request.FirstName;
        customer.LastName = request.LastName;
        customer.Company = request.Company;
        customer.Phone = request.Phone;
        customer.Gender = request.Gender;
        customer.DateOfBirth = request.DateOfBirth;

        await _customerService.UpdateCustomerAsync(customer);

        return Success(await _customerModelFactory.PrepareCustomerProfileModelAsync(customer));
    }

    /// <summary>
    /// Changes the password of the authenticated customer (the current password is verified).
    /// </summary>
    /// <response code="400">The password could not be changed (validation errors).</response>
    [HttpPost("change-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel request)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();

        var changePasswordRequest = new ChangePasswordRequest(
            customer.Email, validateRequest: true, PasswordFormat.Hashed,
            request.NewPassword, request.OldPassword);

        var result = await _customerRegistrationService.ChangePasswordAsync(changePasswordRequest);

        if (!result.Success)
        {
            var details = new Dictionary<string, string[]> { ["errors"] = result.Errors.ToArray() };
            return BadRequest(ApiResponse.Fail("change_password_failed", string.Join("; ", result.Errors), details));
        }

        return Success(new { message = "The password has been changed." });
    }

    #endregion
}
