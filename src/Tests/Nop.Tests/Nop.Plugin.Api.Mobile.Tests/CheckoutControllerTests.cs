using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Api.Mobile.Controllers;
using Nop.Plugin.Api.Mobile.Factories;
using Nop.Plugin.Api.Mobile.Models;
using Nop.Plugin.Api.Mobile.Models.Checkout;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Shipping;
using Nop.Tests.Nop.Services.Tests;
using NUnit.Framework;

namespace Nop.Tests.Nop.Plugin.Api.Mobile.Tests;

[TestFixture]
public class CheckoutControllerTests : ServiceTest
{
    #region Fields

    private IWorkContext _workContext;
    private IStoreContext _storeContext;
    private ICustomerService _customerService;
    private IProductService _productService;
    private IShoppingCartService _shoppingCartService;

    private CheckoutController _checkoutController;
    private Product _product;

    #endregion

    #region SetUp

    [OneTimeSetUp]
    public async Task SetUp()
    {
        _workContext = GetService<IWorkContext>();
        _storeContext = GetService<IStoreContext>();
        _customerService = GetService<ICustomerService>();
        _productService = GetService<IProductService>();
        _shoppingCartService = GetService<IShoppingCartService>();

        var checkoutModelFactory = new CheckoutModelFactory(
            _workContext,
            _storeContext,
            _shoppingCartService,
            GetService<IOrderTotalCalculationService>(),
            GetService<IShippingService>(),
            GetService<IPaymentPluginManager>(),
            GetService<ILocalizationService>(),
            GetService<IPriceFormatter>());

        _checkoutController = new CheckoutController(_workContext, _customerService, checkoutModelFactory);

        _product = new Product
        {
            Name = "API Mobile Checkout Product",
            Sku = "API-MOBILE-CHECKOUT",
            ProductType = ProductType.SimpleProduct,
            Published = true,
            VisibleIndividually = true,
            IsShipEnabled = true,
            Price = 15m,
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

    #endregion

    #region Tests

    [Test]
    public async Task GetCheckoutDataForEmptyCartShouldReturnStructure()
    {
        await AuthenticateFreshCustomerAsync();

        var result = await _checkoutController.Get();

        var data = ExtractSuccess<CheckoutDataModel>(result);
        data.RequiresShipping.Should().BeFalse();
        data.SubTotal.Should().Be(0);
        data.ShippingOptions.Should().NotBeNull();
        data.PaymentMethods.Should().NotBeNull();
    }

    [Test]
    public async Task GetCheckoutDataWithShippableCartShouldRequireShipping()
    {
        var customer = await AuthenticateFreshCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();
        await _shoppingCartService.AddToCartAsync(customer, _product, ShoppingCartType.ShoppingCart, store.Id, quantity: 1);

        var result = await _checkoutController.Get();

        var data = ExtractSuccess<CheckoutDataModel>(result);
        data.RequiresShipping.Should().BeTrue();
        data.SubTotal.Should().BeGreaterThan(0);
    }

    [Test]
    public async Task GetCheckoutDataWithUnknownAddressShouldReturnNotFound()
    {
        await AuthenticateFreshCustomerAsync();

        var result = await _checkoutController.Get(int.MaxValue);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion
}
