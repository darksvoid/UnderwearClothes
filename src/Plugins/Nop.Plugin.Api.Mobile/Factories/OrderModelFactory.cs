using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Api.Mobile.Models;
using Nop.Plugin.Api.Mobile.Models.Orders;
using Nop.Services.Catalog;
using Nop.Services.Orders;

namespace Nop.Plugin.Api.Mobile.Factories;

public class OrderModelFactory : IOrderModelFactory
{
    #region Fields

    protected readonly IOrderService _orderService;
    protected readonly IProductService _productService;
    protected readonly IPriceFormatter _priceFormatter;

    #endregion

    #region Ctor

    public OrderModelFactory(IOrderService orderService,
        IProductService productService,
        IPriceFormatter priceFormatter)
    {
        _orderService = orderService;
        _productService = productService;
        _priceFormatter = priceFormatter;
    }

    #endregion

    #region Methods

    public async Task<OrderOverviewModel> PrepareOrderOverviewModelAsync(Order order)
    {
        return new OrderOverviewModel
        {
            Id = order.Id,
            OrderNumber = GetOrderNumber(order),
            OrderStatus = order.OrderStatus.ToString(),
            PaymentStatus = order.PaymentStatus.ToString(),
            ShippingStatus = order.ShippingStatus.ToString(),
            OrderTotal = order.OrderTotal,
            OrderTotalFormatted = await _priceFormatter.FormatPriceAsync(order.OrderTotal),
            CreatedOnUtc = order.CreatedOnUtc
        };
    }

    public async Task<OrderDetailsModel> PrepareOrderDetailsModelAsync(Order order)
    {
        var items = new List<OrderItemModel>();
        foreach (var orderItem in await _orderService.GetOrderItemsAsync(order.Id))
        {
            var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
            items.Add(new OrderItemModel
            {
                Id = orderItem.Id,
                ProductId = orderItem.ProductId,
                ProductName = product?.Name,
                Quantity = orderItem.Quantity,
                UnitPrice = orderItem.UnitPriceInclTax,
                UnitPriceFormatted = await _priceFormatter.FormatPriceAsync(orderItem.UnitPriceInclTax),
                SubTotal = orderItem.PriceInclTax,
                SubTotalFormatted = await _priceFormatter.FormatPriceAsync(orderItem.PriceInclTax)
            });
        }

        return new OrderDetailsModel
        {
            Id = order.Id,
            OrderNumber = GetOrderNumber(order),
            OrderStatus = order.OrderStatus.ToString(),
            PaymentStatus = order.PaymentStatus.ToString(),
            ShippingStatus = order.ShippingStatus.ToString(),
            OrderTotal = order.OrderTotal,
            OrderTotalFormatted = await _priceFormatter.FormatPriceAsync(order.OrderTotal),
            CreatedOnUtc = order.CreatedOnUtc,
            Items = items
        };
    }

    public async Task<PagedResponse<OrderOverviewModel>> PrepareOrderPagedResponseAsync(IPagedList<Order> orders)
    {
        var items = new List<OrderOverviewModel>();
        foreach (var order in orders)
            items.Add(await PrepareOrderOverviewModelAsync(order));

        return PagedResponse<OrderOverviewModel>.Create(orders, items);
    }

    #endregion

    #region Utilities

    protected static string GetOrderNumber(Order order)
    {
        return string.IsNullOrEmpty(order.CustomOrderNumber)
            ? order.Id.ToString()
            : order.CustomOrderNumber;
    }

    #endregion
}
