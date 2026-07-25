using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Plugin.Api.Mobile.Factories;
using Nop.Plugin.Api.Mobile.Models;
using Nop.Plugin.Api.Mobile.Models.Checkout;
using Nop.Plugin.Api.Mobile.Models.Orders;
using Nop.Plugin.Api.Mobile.Services.Checkout;
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
    protected readonly IOrderModelFactory _orderModelFactory;
    protected readonly IOrderPlacementService _orderPlacementService;

    #endregion

    #region Ctor

    public CheckoutController(IWorkContext workContext,
        ICustomerService customerService,
        ICheckoutModelFactory checkoutModelFactory,
        IOrderModelFactory orderModelFactory,
        IOrderPlacementService orderPlacementService)
    {
        _workContext = workContext;
        _customerService = customerService;
        _checkoutModelFactory = checkoutModelFactory;
        _orderModelFactory = orderModelFactory;
        _orderPlacementService = orderPlacementService;
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

    /// <summary>
    /// Places an order for the authenticated customer using an offline payment method.
    /// </summary>
    /// <response code="400">The order could not be placed (validation errors).</response>
    /// <response code="404">A referenced address was not found for this customer.</response>
    [HttpPost("order")]
    [ProducesResponseType(typeof(ApiResponse<OrderDetailsModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderRequest request)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var addresses = await _customerService.GetAddressesByCustomerIdAsync(customer.Id);

        var billingAddress = addresses.FirstOrDefault(address => address.Id == request.BillingAddressId);
        if (billingAddress is null)
            return NotFoundError("Billing address not found.");

        Address shippingAddress = null;
        if (request.ShippingAddressId.HasValue)
        {
            shippingAddress = addresses.FirstOrDefault(address => address.Id == request.ShippingAddressId.Value);
            if (shippingAddress is null)
                return NotFoundError("Shipping address not found.");
        }

        var (order, errors) = await _orderPlacementService.PlaceOrderAsync(customer, billingAddress, shippingAddress, request);
        if (order is null)
        {
            var details = new Dictionary<string, string[]> { ["errors"] = errors.ToArray() };
            return BadRequest(ApiResponse.Fail("order_error", string.Join("; ", errors), details));
        }

        return Success(await _orderModelFactory.PrepareOrderDetailsModelAsync(order));
    }

    #endregion
}
