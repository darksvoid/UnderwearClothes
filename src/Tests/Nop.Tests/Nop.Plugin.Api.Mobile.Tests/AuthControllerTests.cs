using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Api.Mobile.Controllers;
using Nop.Plugin.Api.Mobile.Domain;
using Nop.Plugin.Api.Mobile.Models;
using Nop.Plugin.Api.Mobile.Models.Auth;
using Nop.Plugin.Api.Mobile.Services.Security;
using Nop.Services.Customers;
using Nop.Tests.Nop.Services.Tests;
using NUnit.Framework;

namespace Nop.Tests.Nop.Plugin.Api.Mobile.Tests;

[TestFixture]
public class AuthControllerTests : ServiceTest
{
    #region Fields

    private IWorkContext _workContext;
    private ICustomerService _customerService;
    private AuthController _authController;

    #endregion

    #region SetUp

    [OneTimeSetUp]
    public void SetUp()
    {
        _workContext = GetService<IWorkContext>();
        _customerService = GetService<ICustomerService>();

        var settings = new ApiMobileSettings { SecretKey = new string('k', 64) };
        var tokenService = new TokenService(settings, TimeProvider.System);
        var blacklistService = new BlacklistService(new MemoryCache(new MemoryCacheOptions()), TimeProvider.System);

        var authenticationService = new ApiAuthenticationService(
            _workContext,
            GetService<IStoreContext>(),
            GetService<ICustomerRegistrationService>(),
            _customerService,
            tokenService,
            blacklistService,
            GetService<CustomerSettings>());

        _authController = new AuthController(authenticationService);
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

    private async Task AuthenticateGuestAsync()
    {
        var customer = new Customer
        {
            CustomerGuid = Guid.NewGuid(),
            Active = true,
            CreatedOnUtc = DateTime.UtcNow,
            LastActivityDateUtc = DateTime.UtcNow
        };
        await _customerService.InsertCustomerAsync(customer);

        var guestRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.GuestsRoleName);
        await _customerService.AddCustomerRoleMappingAsync(new CustomerCustomerRoleMapping
        {
            CustomerId = customer.Id,
            CustomerRoleId = guestRole.Id
        });

        await _workContext.SetCurrentCustomerAsync(customer);
    }

    #endregion

    #region Tests

    [Test]
    public async Task RegisterShouldCreateCustomerAndReturnToken()
    {
        await AuthenticateGuestAsync();
        var email = $"api.reg.{Guid.NewGuid():N}@example.com";

        var result = await _authController.Register(new RegisterRequest
        {
            Email = email,
            Password = "P@ssw0rd1",
            FirstName = "Api",
            LastName = "User"
        });

        var data = ExtractSuccess<RegisterResponse>(result);
        data.Registered.Should().BeTrue();
        data.AccessToken.Should().NotBeNullOrEmpty();

        var created = await _customerService.GetCustomerByEmailAsync(email);
        created.Should().NotBeNull();
        created.FirstName.Should().Be("Api");
    }

    [Test]
    public async Task RegisterWithExistingEmailShouldReturnBadRequest()
    {
        await AuthenticateGuestAsync();

        var result = await _authController.Register(new RegisterRequest
        {
            Email = NopTestsDefaults.AdminEmail,
            Password = "P@ssw0rd1"
        });

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion
}
