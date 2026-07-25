using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Plugin.Api.Mobile.Factories;
using Nop.Plugin.Api.Mobile.Models;
using Nop.Plugin.Api.Mobile.Models.Checkout;
using Nop.Services.Customers;

namespace Nop.Plugin.Api.Mobile.Controllers;

/// <summary>
/// Authenticated checkout data: shipping requirement, cart subtotal, available shipping methods
/// (for a chosen address) and available offline payment methods.
/// </summary>
[Authorize(AuthenticationSchemes = ApiMobileDefaults.AuthenticationScheme)]
public class CheckoutController : BaseApiController
{
    #region Fields

    protected readonly IWorkContext _workContext;
    protected readonly ICustomerService _customerService;
    protected readonly ICheckoutModelFactory _checkoutModelFactory;

    #endregion

    #region Ctor

    public CheckoutController(IWorkContext workContext,
        ICustomerService customerService,
        ICheckoutModelFactory checkoutModelFactory)
    {
        _workContext = workContext;
        _customerService = customerService;
        _checkoutModelFactory = checkoutModelFactory;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Returns the checkout data for the authenticated customer. Pass a shipping address id to get shipping options.
    /// </summary>
    /// <response code="404">The shipping address was not found for this customer.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<CheckoutDataModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get([FromQuery] int? shippingAddressId = null)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();

        Address shippingAddress = null;
        if (shippingAddressId.HasValue)
        {
            var addresses = await _customerService.GetAddressesByCustomerIdAsync(customer.Id);
            shippingAddress = addresses.FirstOrDefault(address => address.Id == shippingAddressId.Value);
            if (shippingAddress is null)
                return NotFoundError("Address not found.");
        }

        return Success(await _checkoutModelFactory.PrepareCheckoutDataAsync(customer, shippingAddress));
    }

    #endregion
}
