using Nop.Core.Domain.Customers;
using Nop.Plugin.Api.Mobile.Models.Cart;

namespace Nop.Plugin.Api.Mobile.Factories;

public interface ICartModelFactory
{
    Task<CartModel> PrepareCartModelAsync(Customer customer);
}
