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
public class CartControllerTests : ServiceTest
{
    #region Fields

    private IWorkContext _workContext;
    private ICustomerService _customerService;
    private IProductService _productService;

    private CartController _cartController;
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

        _cartController = new CartController(
            _workContext,
            GetService<IStoreContext>(),
            GetService<IShoppingCartService>(),
            _productService,
            cartModelFactory);

        _product = new Product
        {
            Name = "API Mobile Test Product",
            Sku = "API-MOBILE-TEST",
            ProductType = ProductType.SimpleProduct,
            Published = true,
            VisibleIndividually = true,
            Price = 12.5m,
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

    private async Task<Customer> AuthenticateFreshCustomerAsync()
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
        return customer;
    }

    private async Task<CartModel> AddProductAsync(int quantity)
    {
        var result = await _cartController.AddItem(new AddCartItemRequest { ProductId = _product.Id, Quantity = quantity });
        return ExtractSuccess<CartModel>(result);
    }

    #endregion

    #region Tests

    [Test]
    public async Task GetEmptyCartShouldReturnNoItems()
    {
        await AuthenticateFreshCustomerAsync();

        var result = await _cartController.Get();

        var cart = ExtractSuccess<CartModel>(result);
        cart.TotalItems.Should().Be(0);
        cart.Items.Should().BeEmpty();
    }

    [Test]
    public async Task AddItemShouldPutProductInCart()
    {
        await AuthenticateFreshCustomerAsync();

        var cart = await AddProductAsync(2);

        cart.TotalItems.Should().Be(2);
        cart.Items.Should().ContainSingle();
        cart.Items[0].ProductId.Should().Be(_product.Id);
        cart.Items[0].Quantity.Should().Be(2);
    }

    [Test]
    public async Task AddUnknownProductShouldReturnNotFound()
    {
        await AuthenticateFreshCustomerAsync();

        var result = await _cartController.AddItem(new AddCartItemRequest { ProductId = int.MaxValue, Quantity = 1 });

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Test]
    public async Task UpdateItemShouldChangeQuantity()
    {
        await AuthenticateFreshCustomerAsync();
        var cart = await AddProductAsync(1);
        var itemId = cart.Items[0].Id;

        var result = await _cartController.UpdateItem(itemId, new UpdateCartItemRequest { Quantity = 5 });

        var updated = ExtractSuccess<CartModel>(result);
        updated.Items.Should().ContainSingle();
        updated.Items[0].Quantity.Should().Be(5);
    }

    [Test]
    public async Task RemoveItemShouldEmptyCart()
    {
        await AuthenticateFreshCustomerAsync();
        var cart = await AddProductAsync(1);
        var itemId = cart.Items[0].Id;

        var result = await _cartController.RemoveItem(itemId);

        var updated = ExtractSuccess<CartModel>(result);
        updated.TotalItems.Should().Be(0);
        updated.Items.Should().BeEmpty();
    }

    [Test]
    public async Task UpdateUnknownItemShouldReturnNotFound()
    {
        await AuthenticateFreshCustomerAsync();

        var result = await _cartController.UpdateItem(int.MaxValue, new UpdateCartItemRequest { Quantity = 3 });

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Test]
    public async Task RemoveUnknownItemShouldReturnNotFound()
    {
        await AuthenticateFreshCustomerAsync();

        var result = await _cartController.RemoveItem(int.MaxValue);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion
}
