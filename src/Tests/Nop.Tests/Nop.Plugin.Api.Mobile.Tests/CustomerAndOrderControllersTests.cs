using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Api.Mobile.Controllers;
using Nop.Plugin.Api.Mobile.Factories;
using Nop.Plugin.Api.Mobile.Models;
using Nop.Plugin.Api.Mobile.Models.Customers;
using Nop.Plugin.Api.Mobile.Models.Orders;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Tests.Nop.Services.Tests;
using NUnit.Framework;

namespace Nop.Tests.Nop.Plugin.Api.Mobile.Tests;

[TestFixture]
public class CustomerAndOrderControllersTests : ServiceTest
{
    #region Fields

    private IWorkContext _workContext;
    private ICustomerService _customerService;
    private IOrderService _orderService;

    private CustomerController _customerController;
    private OrdersController _ordersController;

    private Customer _registeredCustomer;
    private Customer _orderCustomer;
    private Customer _foreignCustomer;
    private int? _sampleOrderId;

    #endregion

    #region SetUp

    [OneTimeSetUp]
    public async Task SetUp()
    {
        _workContext = GetService<IWorkContext>();
        _customerService = GetService<ICustomerService>();
        _orderService = GetService<IOrderService>();

        var customerModelFactory = new CustomerModelFactory(
            _customerService,
            GetService<ICountryService>(),
            GetService<IStateProvinceService>(),
            GetService<ILocalizationService>());

        var orderModelFactory = new OrderModelFactory(
            _orderService,
            GetService<IProductService>(),
            GetService<IPriceFormatter>());

        _customerController = new CustomerController(_workContext, _customerService, GetService<IAddressService>(), customerModelFactory);
        _ordersController = new OrdersController(_workContext, _orderService, orderModelFactory);

        _registeredCustomer = await _customerService.GetCustomerByEmailAsync(NopTestsDefaults.AdminEmail);

        var anyOrder = (await _orderService.SearchOrdersAsync(pageSize: 1)).FirstOrDefault();
        if (anyOrder != null)
        {
            _orderCustomer = await _customerService.GetCustomerByIdAsync(anyOrder.CustomerId);
            _sampleOrderId = anyOrder.Id;
        }

        _foreignCustomer = new Customer
        {
            CustomerGuid = Guid.NewGuid(),
            Active = true,
            CreatedOnUtc = DateTime.UtcNow,
            LastActivityDateUtc = DateTime.UtcNow
        };
        await _customerService.InsertCustomerAsync(_foreignCustomer);
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

    private async Task AuthenticateAsync(Customer customer)
    {
        await _workContext.SetCurrentCustomerAsync(customer);
    }

    #endregion

    #region Profile & addresses

    [Test]
    public async Task GetProfileShouldReturnCurrentCustomer()
    {
        await AuthenticateAsync(_registeredCustomer);

        var result = await _customerController.GetProfile();

        var data = ExtractSuccess<CustomerProfileModel>(result);
        data.Id.Should().Be(_registeredCustomer.Id);
        data.Email.Should().Be(_registeredCustomer.Email);
    }

    [Test]
    public async Task GetAddressesShouldReturnList()
    {
        await AuthenticateAsync(_registeredCustomer);

        var result = await _customerController.GetAddresses();

        var data = ExtractSuccess<List<AddressModel>>(result);
        data.Should().NotBeNull();
    }

    [Test]
    public async Task GetUnknownAddressShouldReturnNotFound()
    {
        await AuthenticateAsync(_registeredCustomer);

        var result = await _customerController.GetAddress(int.MaxValue);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region Orders

    [Test]
    public async Task GetOrdersShouldReturnCustomerOrders()
    {
        if (_orderCustomer is null)
            Assert.Ignore("No sample orders were seeded.");

        await AuthenticateAsync(_orderCustomer);

        var result = await _ordersController.GetOrders();

        var data = ExtractSuccess<PagedResponse<OrderOverviewModel>>(result);
        data.TotalCount.Should().BeGreaterThan(0);
        data.Items.Should().NotBeEmpty();
    }

    [Test]
    public async Task GetOrderDetailsShouldReturnOwnedOrder()
    {
        if (_orderCustomer is null || !_sampleOrderId.HasValue)
            Assert.Ignore("No sample orders were seeded.");

        await AuthenticateAsync(_orderCustomer);

        var result = await _ordersController.Get(_sampleOrderId.Value);

        var data = ExtractSuccess<OrderDetailsModel>(result);
        data.Id.Should().Be(_sampleOrderId.Value);
        data.Items.Should().NotBeNull();
    }

    [Test]
    public async Task GetOrderOwnedByAnotherCustomerShouldReturnNotFound()
    {
        if (!_sampleOrderId.HasValue)
            Assert.Ignore("No sample orders were seeded.");

        await AuthenticateAsync(_foreignCustomer);

        var result = await _ordersController.Get(_sampleOrderId.Value);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Test]
    public async Task GetUnknownOrderShouldReturnNotFound()
    {
        await AuthenticateAsync(_registeredCustomer);

        var result = await _ordersController.Get(int.MaxValue);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region Address management

    private async Task AuthenticateNewCustomerAsync()
    {
        var customer = new Customer
        {
            CustomerGuid = Guid.NewGuid(),
            Active = true,
            CreatedOnUtc = DateTime.UtcNow,
            LastActivityDateUtc = DateTime.UtcNow
        };
        await _customerService.InsertCustomerAsync(customer);
        await _workContext.SetCurrentCustomerAsync(customer);
    }

    private static async Task<AddressRequest> BuildValidAddressAsync()
    {
        var country = (await GetService<ICountryService>().GetAllCountriesAsync()).First();
        var states = await GetService<IStateProvinceService>().GetStateProvincesByCountryIdAsync(country.Id);

        return new AddressRequest
        {
            FirstName = "Api",
            LastName = "Tester",
            Email = "api.address@example.com",
            Address1 = "Test Street 1",
            City = "Test City",
            ZipPostalCode = "10001",
            PhoneNumber = "1234567890",
            CountryId = country.Id,
            StateProvinceId = states.FirstOrDefault()?.Id
        };
    }

    [Test]
    public async Task CreateAddressShouldAddToCustomer()
    {
        await AuthenticateNewCustomerAsync();

        var result = await _customerController.CreateAddress(await BuildValidAddressAsync());

        var data = ExtractSuccess<AddressModel>(result);
        data.Id.Should().BeGreaterThan(0);
        data.City.Should().Be("Test City");
    }

    [Test]
    public async Task CreateInvalidAddressShouldReturnBadRequest()
    {
        await AuthenticateNewCustomerAsync();

        var result = await _customerController.CreateAddress(new AddressRequest());

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Test]
    public async Task UpdateAddressShouldChangeFields()
    {
        await AuthenticateNewCustomerAsync();
        var created = ExtractSuccess<AddressModel>(await _customerController.CreateAddress(await BuildValidAddressAsync()));

        var request = await BuildValidAddressAsync();
        request.City = "Updated City";
        var result = await _customerController.UpdateAddress(created.Id, request);

        var data = ExtractSuccess<AddressModel>(result);
        data.Id.Should().Be(created.Id);
        data.City.Should().Be("Updated City");
    }

    [Test]
    public async Task UpdateUnknownAddressShouldReturnNotFound()
    {
        await AuthenticateNewCustomerAsync();

        var result = await _customerController.UpdateAddress(int.MaxValue, await BuildValidAddressAsync());

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Test]
    public async Task DeleteAddressShouldRemoveFromCustomer()
    {
        await AuthenticateNewCustomerAsync();
        var customer = await _workContext.GetCurrentCustomerAsync();
        var created = ExtractSuccess<AddressModel>(await _customerController.CreateAddress(await BuildValidAddressAsync()));

        await _customerController.DeleteAddress(created.Id);

        var addresses = await _customerService.GetAddressesByCustomerIdAsync(customer.Id);
        addresses.Any(address => address.Id == created.Id).Should().BeFalse();
    }

    [Test]
    public async Task DeleteUnknownAddressShouldReturnNotFound()
    {
        await AuthenticateNewCustomerAsync();

        var result = await _customerController.DeleteAddress(int.MaxValue);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion
}
