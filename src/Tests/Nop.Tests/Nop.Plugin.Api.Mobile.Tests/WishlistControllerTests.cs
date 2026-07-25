using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Api.Mobile.Controllers;
using Nop.Plugin.Api.Mobile.Factories;
using Nop.Plugin.Api.Mobile.Models;
using Nop.Plugin.Api.Mobile.Models.Cart;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Orders;
using Nop.Tests.Nop.Services.Tests;
using NUnit.Framework;

namespace Nop.Tests.Nop.Plugin.Api.Mobile.Tests;

[TestFixture]
public class WishlistControllerTests : ServiceTest
{
    #region Fields

    private IWorkContext _workContext;
    private ICustomerService _customerService;
    private IProductService _productService;

    private WishlistController _wishlistController;
    private Product _product;

    #endregion

    #region SetUp

    [OneTimeSetUp]
    public async Task SetUp()
    {
        _workContext = GetService<IWorkContext>();
        _customerService = GetService<ICustomerService>();
        _productService = GetService<IProductService>();

        var cartModelFactory = new CartModelFactory(
            GetService<IStoreContext>(),
            GetService<IShoppingCartService>(),
            GetService<IOrderTotalCalculationService>(),
            _productService,
            GetService<IPriceFormatter>());

        _wishlistController = new WishlistController(
            _workContext,
            GetService<IStoreContext>(),
            GetService<IShoppingCartService>(),
            _productService,
            cartModelFactory);

        _product = new Product
        {
            Name = "API Mobile Wishlist Product",
            Sku = "API-MOBILE-WISH",
            ProductType = ProductType.SimpleProduct,
            Published = true,
            VisibleIndividually = true,
            Price = 20m,
            OrderMinimumQuantity = 1,
            OrderMaximumQuantity = 10000,
            ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
            CreatedOnUtc = DateTime.UtcNow,
            UpdatedOnUtc = DateTime.UtcNow
        };
        await _productService.InsertProductAsync(_product);
    }

    #endregion

    #region Utilities

    private static T ExtractSuccess<T>(IActionResult result)
    {
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();

        var response = okResult.Value as ApiResponse<T>;
        response.Should().NotBeNull();
        response.Success.Should().BeTrue();
        response.Error.Should().BeNull();

        return response.Data;
    }

    private async Task AuthenticateFreshCustomerAsync()
    {
        var customer = new Customer
        {
            CustomerGuid = Guid.NewGuid(),
            Active = true,
            CreatedOnUtc = DateTime.UtcNow,
            LastActivityDateUtc = DateTime.UtcNow
        };
        await _customerService.InsertCustomerAsync(customer);

        var registeredRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.RegisteredRoleName);
        await _customerService.AddCustomerRoleMappingAsync(new CustomerCustomerRoleMapping
        {
            CustomerId = customer.Id,
            CustomerRoleId = registeredRole.Id
        });

        await _workContext.SetCurrentCustomerAsync(customer);
    }

    private async Task<WishlistModel> AddProductAsync()
    {
        var result = await _wishlistController.AddItem(new AddCartItemRequest { ProductId = _product.Id, Quantity = 1 });
        return ExtractSuccess<WishlistModel>(result);
    }

    #endregion

    #region Tests

    [Test]
    public async Task GetEmptyWishlistShouldReturnNoItems()
    {
        await AuthenticateFreshCustomerAsync();

        var result = await _wishlistController.Get();

        var wishlist = ExtractSuccess<WishlistModel>(result);
        wishlist.TotalItems.Should().Be(0);
        wishlist.Items.Should().BeEmpty();
    }

    [Test]
    public async Task AddItemShouldPutProductInWishlist()
    {
        await AuthenticateFreshCustomerAsync();

        var wishlist = await AddProductAsync();

        wishlist.Items.Should().ContainSingle();
        wishlist.Items[0].ProductId.Should().Be(_product.Id);
    }

    [Test]
    public async Task AddUnknownProductShouldReturnNotFound()
    {
        await AuthenticateFreshCustomerAsync();

        var result = await _wishlistController.AddItem(new AddCartItemRequest { ProductId = int.MaxValue, Quantity = 1 });

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Test]
    public async Task RemoveItemShouldEmptyWishlist()
    {
        await AuthenticateFreshCustomerAsync();
        var wishlist = await AddProductAsync();
        var itemId = wishlist.Items[0].Id;

        var result = await _wishlistController.RemoveItem(itemId);

        var updated = ExtractSuccess<WishlistModel>(result);
        updated.TotalItems.Should().Be(0);
        updated.Items.Should().BeEmpty();
    }

    [Test]
    public async Task RemoveUnknownItemShouldReturnNotFound()
    {
        await AuthenticateFreshCustomerAsync();

        var result = await _wishlistController.RemoveItem(int.MaxValue);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion
}
