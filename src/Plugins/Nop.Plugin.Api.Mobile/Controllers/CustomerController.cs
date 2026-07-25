using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Api.Mobile.Factories;
using Nop.Plugin.Api.Mobile.Models;
using Nop.Plugin.Api.Mobile.Models.Customers;
using Nop.Services.Customers;

namespace Nop.Plugin.Api.Mobile.Controllers;

/// <summary>
/// Authenticated customer account: profile and addresses (read-only).
/// </summary>
[Authorize(AuthenticationSchemes = ApiMobileDefaults.AuthenticationScheme)]
public class CustomerController : BaseApiController
{
    #region Fields

    protected readonly IWorkContext _workContext;
    protected readonly ICustomerService _customerService;
    protected readonly ICustomerModelFactory _customerModelFactory;

    #endregion

    #region Ctor

    public CustomerController(IWorkContext workContext,
        ICustomerService customerService,
        ICustomerModelFactory customerModelFactory)
    {
        _workContext = workContext;
        _customerService = customerService;
        _customerModelFactory = customerModelFactory;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Returns the profile of the authenticated customer.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<CustomerProfileModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProfile()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        return Success(await _customerModelFactory.PrepareCustomerProfileModelAsync(customer));
    }

    /// <summary>
    /// Returns the addresses of the authenticated customer.
    /// </summary>
    [HttpGet("addresses")]
    [ProducesResponseType(typeof(ApiResponse<IList<AddressModel>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAddresses()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var addresses = await _customerService.GetAddressesByCustomerIdAsync(customer.Id);

        var models = new List<AddressModel>();
        foreach (var address in addresses)
            models.Add(await _customerModelFactory.PrepareAddressModelAsync(address));

        return Success(models);
    }

    /// <summary>
    /// Returns a single address of the authenticated customer.
    /// </summary>
    /// <response code="404">The address was not found for this customer.</response>
    [HttpGet("addresses/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<AddressModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAddress(int id)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var addresses = await _customerService.GetAddressesByCustomerIdAsync(customer.Id);

        var address = addresses.FirstOrDefault(a => a.Id == id);
        if (address is null)
            return NotFoundError("Address not found.");

        return Success(await _customerModelFactory.PrepareAddressModelAsync(address));
    }

    #endregion
}
