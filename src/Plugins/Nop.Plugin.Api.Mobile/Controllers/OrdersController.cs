using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Api.Mobile.Factories;
using Nop.Plugin.Api.Mobile.Models;
using Nop.Plugin.Api.Mobile.Models.Orders;
using Nop.Services.Orders;

namespace Nop.Plugin.Api.Mobile.Controllers;

/// <summary>
/// Authenticated customer order history (read-only).
/// </summary>
[Authorize(AuthenticationSchemes = ApiMobileDefaults.AuthenticationScheme)]
public class OrdersController : BaseApiController
{
    #region Fields

    protected readonly IWorkContext _workContext;
    protected readonly IOrderService _orderService;
    protected readonly IOrderModelFactory _orderModelFactory;

    #endregion

    #region Ctor

    public OrdersController(IWorkContext workContext,
        IOrderService orderService,
        IOrderModelFactory orderModelFactory)
    {
        _workContext = workContext;
        _orderService = orderService;
        _orderModelFactory = orderModelFactory;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Returns the orders of the authenticated customer (paged).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<OrderOverviewModel>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrders([FromQuery] int pageIndex = 0, [FromQuery] int pageSize = ApiMobileDefaults.DefaultPageSize)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();

        var orders = await _orderService.SearchOrdersAsync(
            customerId: customer.Id,
            pageIndex: NormalizePageIndex(pageIndex),
            pageSize: NormalizePageSize(pageSize));

        return Success(await _orderModelFactory.PrepareOrderPagedResponseAsync(orders));
    }

    /// <summary>
    /// Returns the details of an order that belongs to the authenticated customer.
    /// </summary>
    /// <response code="404">The order was not found or does not belong to this customer.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<OrderDetailsModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(int id)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();

        var order = await _orderService.GetOrderByIdAsync(id);
        if (order is null || order.Deleted || order.CustomerId != customer.Id)
            return NotFoundError("Order not found.");

        return Success(await _orderModelFactory.PrepareOrderDetailsModelAsync(order));
    }

    #endregion
}
