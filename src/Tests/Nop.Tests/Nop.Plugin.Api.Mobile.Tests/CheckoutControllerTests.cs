using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Api.Mobile.Controllers;
using Nop.Plugin.Api.Mobile.Factories;
using Nop.Plugin.Api.Mobile.Models;
using Nop.Plugin.Api.Mobile.Models.Checkout;
using Nop.Plugin.Api.Mobile.Models.Orders;
using Nop.Plugin.Api.Mobile.Services.Checkout;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
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
    #region Constants

    private const string OfflinePaymentMethod = "Payments.TestMethod";

    #endregion

    #region Fields

    private IWorkContext _workContext;
    private IStoreContext _storeContext;
    private ICustomerService _customerService;
    private IAddressService _addressService;
    private IProductService _productService;
    private IShoppingCartService _shoppingCartService;

    private CheckoutController _checkoutController;
    private Product _shippableProduct;
    private Product _digitalProduct;

    #endregion

    #region SetUp

    [OneTimeSetUp]
    public async Task SetUp()
    {
        _workContext = GetService<IWorkContext>();
        _storeContext = GetService<IStoreContext>();
        _customerService = GetService<ICustomerService>();
        _addressService = GetService<IAddressService>();
        _productService = GetService<IProductService>();
        _shoppingCartService = GetService<IShoppingCartService>();

        //activate the offline test payment method BEFORE resolving services that capture PaymentSettings
        var paymentSettings = GetService<PaymentSettings>();
        if (!paymentSettings.ActivePaymentMethodSystemNames.Contains(OfflinePaymentMethod))
        {
            paymentSettings.ActivePaymentMethodSystemNames.Add(OfflinePaymentMethod);
            await GetService<ISettingService>().SaveSettingAsync(paymentSettings);
        }

        var checkoutModelFactory = new CheckoutModelFactory(
            _workContext, _storeContext, _shoppingCartService,
            GetService<IOrderTotalCalculationService>(),
            GetService<IShippingService>(),
            GetService<IPaymentPluginManager>(),
            GetService<ILocalizationService>(),
            GetService<IPriceFormatter>());

        var orderModelFactory = new OrderModelFactory(
            GetService<IOrderService>(), _productService, GetService<IPriceFormatter>());

        var orderPlacementService = new OrderPlacementService(
            _storeContext, _shoppingCartService, _customerService,
            GetService<IShippingService>(),
            GetService<IGenericAttributeService>(),
            GetService<IPaymentPluginManager>(),
            GetService<IOrderProcessingService>());

        _checkoutController = new CheckoutController(_workContext, _customerService, checkoutModelFactory, orderModelFactory, orderPlacementService);

        _shippableProduct = await CreateProductAsync("API Mobile Checkout Shippable", "API-MOBILE-CHECKOUT-SHIP", isShipEnabled: true);
        _digitalProduct = await CreateProductAsync("API Mobile Checkout Digital", "API-MOBILE-CHECKOUT-DIGITAL", isShipEnabled: false);
    }

    #endregion

    #region Utilities

    private async Task<Product> CreateProductAsync(string name, string sku, bool isShipEnabled)
    {
        var product = new Product
        {
            Name = name,
            Sku = sku,
            ProductType = ProductType.SimpleProduct,
            Published = true,
            VisibleIndividually = true,
            IsShipEnabled = isShipEnabled,
            Price = 15m,
            OrderMinimumQuantity = 1,
            OrderMaximumQuantity = 10000,
            ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
            CreatedOnUtc = DateTime.UtcNow,
            UpdatedOnUtc = DateTime.UtcNow
        };
        await _productService.InsertProductAsync(product);
        return product;
    }

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

    private async Task<int> CreateBillingAddressAsync(Customer customer)
    {
        var country = (await GetService<ICountryService>().GetAllCountriesAsync()).First();
        var states = await GetService<IStateProvinceService>().GetStateProvincesByCountryIdAsync(country.Id);

        var address = new Address
        {
            FirstName = "Api",
            LastName = "Buyer",
            Email = "api.buyer@example.com",
            Address1 = "Test Street 1",
            City = "Test City",
            ZipPostalCode = "10001",
            PhoneNumber = "1234567890",
            CountryId = country.Id,
            StateProvinceId = states.FirstOrDefault()?.Id,
            CreatedOnUtc = DateTime.UtcNow
        };
        await _addressService.InsertAddressAsync(address);
        await _customerService.InsertCustomerAddressAsync(customer, address);
        return address.Id;
    }

    private async Task AddToCartAsync(Customer customer, Product product)
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        await _shoppingCartService.AddToCartAsync(customer, product, ShoppingCartType.ShoppingCart, store.Id, quantity: 1);
    }

    #endregion

    #region Checkout data

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
        await AddToCartAsync(customer, _shippableProduct);

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

    #region Place order

    [Test]
    public async Task PlaceOrderWithOfflinePaymentShouldCreateOrder()
    {
        var customer = await AuthenticateFreshCustomerAsync();
        var billingAddressId = await CreateBillingAddressAsync(customer);
        await AddToCartAsync(customer, _digitalProduct);

        var result = await _checkoutController.PlaceOrder(new PlaceOrderRequest
        {
            BillingAddressId = billingAddressId,
            PaymentMethodSystemName = OfflinePaymentMethod
        });

        if (result is not OkObjectResult)
        {
            var error = (result as ObjectResult)?.Value as ApiResponse<object>;
            Assert.Fail($"PlaceOrder returned {result.GetType().Name}: {error?.Error?.Message}");
        }

        var data = ExtractSuccess<OrderDetailsModel>(result);
        data.Id.Should().BeGreaterThan(0);
        data.Items.Should().ContainSingle(item => item.ProductId == _digitalProduct.Id);
    }

    [Test]
    public async Task PlaceOrderWithUnknownBillingAddressShouldReturnNotFound()
    {
        await AuthenticateFreshCustomerAsync();

        var result = await _checkoutController.PlaceOrder(new PlaceOrderRequest
        {
            BillingAddressId = int.MaxValue,
            PaymentMethodSystemName = OfflinePaymentMethod
        });

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Test]
    public async Task PlaceOrderWithEmptyCartShouldReturnBadRequest()
    {
        var customer = await AuthenticateFreshCustomerAsync();
        var billingAddressId = await CreateBillingAddressAsync(customer);

        var result = await _checkoutController.PlaceOrder(new PlaceOrderRequest
        {
            BillingAddressId = billingAddressId,
            PaymentMethodSystemName = OfflinePaymentMethod
        });

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Test]
    public async Task PlaceOrderWithInactivePaymentShouldReturnBadRequest()
    {
        var customer = await AuthenticateFreshCustomerAsync();
        var billingAddressId = await CreateBillingAddressAsync(customer);
        await AddToCartAsync(customer, _digitalProduct);

        var result = await _checkoutController.PlaceOrder(new PlaceOrderRequest
        {
            BillingAddressId = billingAddressId,
            PaymentMethodSystemName = "Payments.DoesNotExist"
        });

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion
}
