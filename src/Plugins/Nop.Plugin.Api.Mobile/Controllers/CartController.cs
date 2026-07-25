using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Api.Mobile.Factories;
using Nop.Plugin.Api.Mobile.Models;
using Nop.Plugin.Api.Mobile.Models.Cart;
using Nop.Services.Catalog;
using Nop.Services.Orders;

namespace Nop.Plugin.Api.Mobile.Controllers;

/// <summary>
/// Authenticated shopping cart: view, add, update quantity and remove items.
/// </summary>
[Authorize(AuthenticationSchemes = ApiMobileDefaults.AuthenticationScheme)]
public class CartController : BaseApiController
{
    #region Fields

    protected readonly IWorkContext _workContext;
    protected readonly IStoreContext _storeContext;
    protected readonly IShoppingCartService _shoppingCartService;
    protected readonly IProductService _productService;
    protected readonly ICartModelFactory _cartModelFactory;

    #endregion

    #region Ctor

    public CartController(IWorkContext workContext,
        IStoreContext storeContext,
        IShoppingCartService shoppingCartService,
        IProductService productService,
        ICartModelFactory cartModelFactory)
    {
        _workContext = workContext;
        _storeContext = storeContext;
        _shoppingCartService = shoppingCartService;
        _productService = productService;
        _cartModelFactory = cartModelFactory;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Returns the current shopping cart of the authenticated customer.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<CartModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        return Success(await _cartModelFactory.PrepareCartModelAsync(customer));
    }

    /// <summary>
    /// Adds a product to the shopping cart and returns the updated cart.
    /// </summary>
    /// <response code="400">The product could not be added to the cart.</response>
    /// <response code="404">The product was not found.</response>
    [HttpPost("items")]
    [ProducesResponseType(typeof(ApiResponse<CartModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddItem([FromBody] AddCartItemRequest request)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();

        var product = await _productService.GetProductByIdAsync(request.ProductId);
        if (product is null || product.Deleted || !product.Published)
            return NotFoundError("Product not found.");

        var warnings = await _shoppingCartService.AddToCartAsync(
            customer, product, ShoppingCartType.ShoppingCart, store.Id, quantity: request.Quantity);

        if (warnings.Any())
            return CartError(warnings);

        return Success(await _cartModelFactory.PrepareCartModelAsync(customer));
    }

    /// <summary>
    /// Updates the quantity of a cart item and returns the updated cart.
    /// </summary>
    /// <response code="400">The quantity could not be updated.</response>
    /// <response code="404">The cart item was not found.</response>
    [HttpPut("items/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CartModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateItem(int id, [FromBody] UpdateCartItemRequest request)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var item = await GetOwnedCartItemAsync(customer, id);
        if (item is null)
            return NotFoundError("Cart item not found.");

        var warnings = await _shoppingCartService.UpdateShoppingCartItemAsync(
            customer, item.Id, item.AttributesXml, item.CustomerEnteredPrice, quantity: request.Quantity);

        if (warnings.Any())
            return CartError(warnings);

        return Success(await _cartModelFactory.PrepareCartModelAsync(customer));
    }

    /// <summary>
    /// Removes an item from the cart and returns the updated cart.
    /// </summary>
    /// <response code="404">The cart item was not found.</response>
    [HttpDelete("items/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CartModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveItem(int id)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var item = await GetOwnedCartItemAsync(customer, id);
        if (item is null)
            return NotFoundError("Cart item not found.");

        await _shoppingCartService.DeleteShoppingCartItemAsync(item.Id);

        return Success(await _cartModelFactory.PrepareCartModelAsync(customer));
    }

    #endregion

    #region Utilities

    private async Task<ShoppingCartItem> GetOwnedCartItemAsync(Nop.Core.Domain.Customers.Customer customer, int itemId)
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);
        return cart.FirstOrDefault(item => item.Id == itemId);
    }

    private IActionResult CartError(IList<string> warnings)
    {
        var details = new Dictionary<string, string[]> { ["warnings"] = warnings.ToArray() };
        return BadRequest(ApiResponse.Fail("cart_error", string.Join("; ", warnings), details));
    }

    #endregion
}
