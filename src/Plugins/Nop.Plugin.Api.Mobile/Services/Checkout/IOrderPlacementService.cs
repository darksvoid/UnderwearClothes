using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Api.Mobile.Models.Checkout;

namespace Nop.Plugin.Api.Mobile.Services.Checkout;

public interface IOrderPlacementService
{
    Task<(Order order, IList<string> errors)> PlaceOrderAsync(Customer customer, Address billingAddress, Address shippingAddress, PlaceOrderRequest request);
}
