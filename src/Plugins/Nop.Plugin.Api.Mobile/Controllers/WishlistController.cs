using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Api.Mobile.Factories;
using Nop.Plugin.Api.Mobile.Models;
using Nop.Plugin.Api.Mobile.Models.Cart;
using Nop.Services.Catalog;
using Nop.Services.Orders;

namespace Nop.Plugin.Api.Mobile.Controllers;

/// <summary>
/// Authenticated wishlist: view, add and remove items.
/// </summary>
[Authorize(AuthenticationSchemes = ApiMobileDefaults.AuthenticationScheme)]
public class WishlistController : BaseApiController
{
    #region Fields

    protected readonly IWorkContext _workContext;
    protected readonly IStoreContext _storeContext;
    protected readonly IShoppingCartService _shoppingCartService;
    protected readonly IProductService _productService;
    protected readonly ICartModelFactory _cartModelFactory;

    #endregion

    #region Ctor

    public WishlistController(IWorkContext workContext,
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
    /// Returns the current wishlist of the authenticated customer.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<WishlistModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        return Success(await _cartModelFactory.PrepareWishlistModelAsync(customer));
    }

    /// <summary>
    /// Adds a product to the wishlist and returns the updated wishlist.
    /// </summary>
    /// <response code="400">The product could not be added to the wishlist.</response>
    /// <response code="404">The product was not found.</response>
    [HttpPost("items")]
    [ProducesResponseType(typeof(ApiResponse<WishlistModel>), StatusCodes.Status200OK)]
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
            customer, product, ShoppingCartType.Wishlist, store.Id, quantity: request.Quantity);

        if (warnings.Any())
        {
            var details = new Dictionary<string, string[]> { ["warnings"] = warnings.ToArray() };
            return BadRequest(ApiResponse.Fail("wishlist_error", string.Join("; ", warnings), details));
        }

        return Success(await _cartModelFactory.PrepareWishlistModelAsync(customer));
    }

    /// <summary>
    /// Removes an item from the wishlist and returns the updated wishlist.
    /// </summary>
    /// <response code="404">The wishlist item was not found.</response>
    [HttpDelete("items/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<WishlistModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveItem(int id)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();

        var wishlist = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.Wishlist, store.Id);
        var item = wishlist.FirstOrDefault(wishlistItem => wishlistItem.Id == id);
        if (item is null)
            return NotFoundError("Wishlist item not found.");

        await _shoppingCartService.DeleteShoppingCartItemAsync(item.Id);

        return Success(await _cartModelFactory.PrepareWishlistModelAsync(customer));
    }

    #endregion
}
