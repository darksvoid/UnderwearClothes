using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Api.Mobile.Models.Cart;
using Nop.Services.Catalog;
using Nop.Services.Orders;

namespace Nop.Plugin.Api.Mobile.Factories;

public class CartModelFactory : ICartModelFactory
{
    #region Fields

    protected readonly IStoreContext _storeContext;
    protected readonly IShoppingCartService _shoppingCartService;
    protected readonly IOrderTotalCalculationService _orderTotalCalculationService;
    protected readonly IProductService _productService;
    protected readonly IPriceFormatter _priceFormatter;

    #endregion

    #region Ctor

    public CartModelFactory(IStoreContext storeContext,
        IShoppingCartService shoppingCartService,
        IOrderTotalCalculationService orderTotalCalculationService,
        IProductService productService,
        IPriceFormatter priceFormatter)
    {
        _storeContext = storeContext;
        _shoppingCartService = shoppingCartService;
        _orderTotalCalculationService = orderTotalCalculationService;
        _productService = productService;
        _priceFormatter = priceFormatter;
    }

    #endregion

    #region Methods

    public async Task<CartModel> PrepareCartModelAsync(Customer customer)
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

        var items = new List<CartItemModel>();
        foreach (var shoppingCartItem in cart)
        {
            var product = await _productService.GetProductByIdAsync(shoppingCartItem.ProductId);
            var (unitPrice, _, _) = await _shoppingCartService.GetUnitPriceAsync(shoppingCartItem, true);
            var (subTotal, _, _, _) = await _shoppingCartService.GetSubTotalAsync(shoppingCartItem, true);

            items.Add(new CartItemModel
            {
                Id = shoppingCartItem.Id,
                ProductId = shoppingCartItem.ProductId,
                ProductName = product?.Name,
                Quantity = shoppingCartItem.Quantity,
                UnitPrice = unitPrice,
                UnitPriceFormatted = await _priceFormatter.FormatPriceAsync(unitPrice),
                SubTotal = subTotal,
                SubTotalFormatted = await _priceFormatter.FormatPriceAsync(subTotal)
            });
        }

        var (_, _, _, subTotalWithDiscount, _) = await _orderTotalCalculationService.GetShoppingCartSubTotalAsync(cart, true);

        return new CartModel
        {
            Items = items,
            TotalItems = cart.Sum(item => item.Quantity),
            SubTotal = subTotalWithDiscount,
            SubTotalFormatted = await _priceFormatter.FormatPriceAsync(subTotalWithDiscount)
        };
    }

    #endregion
}
