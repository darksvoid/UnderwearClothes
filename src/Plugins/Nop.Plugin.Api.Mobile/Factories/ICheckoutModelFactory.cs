using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Api.Mobile.Models.Checkout;

namespace Nop.Plugin.Api.Mobile.Factories;

public interface ICheckoutModelFactory
{
    Task<CheckoutDataModel> PrepareCheckoutDataAsync(Customer customer, Address shippingAddress);
}
