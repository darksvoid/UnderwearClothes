using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Api.Mobile.Controllers;
using Nop.Plugin.Api.Mobile.Factories;
using Nop.Plugin.Api.Mobile.Models;
using Nop.Plugin.Api.Mobile.Models.Account;
using Nop.Plugin.Api.Mobile.Models.Customers;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Tests.Nop.Services.Tests;
using NUnit.Framework;

namespace Nop.Tests.Nop.Plugin.Api.Mobile.Tests;

[TestFixture]
public class AccountControllerTests : ServiceTest
{
    #region Fields

    private IWorkContext _workContext;
    private IStoreContext _storeContext;
    private ICustomerService _customerService;
    private ICustomerRegistrationService _customerRegistrationService;
    private AccountController _accountController;

    #endregion

    #region SetUp

    [OneTimeSetUp]
    public void SetUp()
    {
        _workContext = GetService<IWorkContext>();
        _storeContext = GetService<IStoreContext>();
        _customerService = GetService<ICustomerService>();
        _customerRegistrationService = GetService<ICustomerRegistrationService>();

        var customerModelFactory = new CustomerModelFactory(
            _customerService,
            GetService<ICountryService>(),
            GetService<IStateProvinceService>(),
            GetService<ILocalizationService>());

        _accountController = new AccountController(
            _workContext, _customerService, _customerRegistrationService, customerModelFactory);
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

    private async Task<(Customer customer, string password)> RegisterAndAuthenticateAsync()
    {
        var email = $"acc.{Guid.NewGuid():N}@example.com";
        const string password = "P@ssw0rd1";

        var customer = await _customerService.InsertGuestCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();

        var registration = await _customerRegistrationService.RegisterCustomerAsync(new CustomerRegistrationRequest(
            customer, email, email, password, PasswordFormat.Hashed, store.Id, isApproved: true));
        registration.Success.Should().BeTrue();

        await _workContext.SetCurrentCustomerAsync(customer);
        return (customer, password);
    }

    #endregion

    #region Tests

    [Test]
    public async Task UpdateProfileShouldChangeFields()
    {
        await RegisterAndAuthenticateAsync();

        var result = await _accountController.UpdateProfile(new UpdateProfileRequest
        {
            FirstName = "Пётр",
            LastName = "Иванов",
            Phone = "+7 900 000-00-00"
        });

        var data = ExtractSuccess<CustomerProfileModel>(result);
        data.FirstName.Should().Be("Пётр");
        data.LastName.Should().Be("Иванов");
        data.Phone.Should().Be("+7 900 000-00-00");
    }

    [Test]
    public async Task ChangePasswordWithCorrectOldPasswordShouldSucceed()
    {
        var (_, password) = await RegisterAndAuthenticateAsync();

        var result = await _accountController.ChangePassword(new ChangePasswordModel
        {
            OldPassword = password,
            NewPassword = "N3wP@ssw0rd"
        });

        result.Should().BeOfType<OkObjectResult>();
    }

    [Test]
    public async Task ChangePasswordWithWrongOldPasswordShouldReturnBadRequest()
    {
        await RegisterAndAuthenticateAsync();

        var result = await _accountController.ChangePassword(new ChangePasswordModel
        {
            OldPassword = "wrong-password",
            NewPassword = "N3wP@ssw0rd"
        });

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion
}
