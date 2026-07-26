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

    private ICustomerService _customerService;
    private AuthController _authController;

    #endregion

    #region SetUp

    [OneTimeSetUp]
    public void SetUp()
    {
        _customerService = GetService<ICustomerService>();

        var settings = new ApiMobileSettings { SecretKey = new string('k', 64) };
        var tokenService = new TokenService(settings, TimeProvider.System);
        var blacklistService = new BlacklistService(new MemoryCache(new MemoryCacheOptions()), TimeProvider.System);

        var authenticationService = new ApiAuthenticationService(
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

    private static RegisterRequest NewRegistration()
    {
        return new RegisterRequest
        {
            Email = $"api.reg.{Guid.NewGuid():N}@example.com",
            Password = "P@ssw0rd1",
            FirstName = "Api",
            LastName = "User"
        };
    }

    #endregion

    #region Tests

    [Test]
    public async Task RegisterShouldCreateCustomerAndReturnToken()
    {
        var request = NewRegistration();

        var result = await _authController.Register(request);

        var data = ExtractSuccess<RegisterResponse>(result);
        data.Registered.Should().BeTrue();
        data.AccessToken.Should().NotBeNullOrEmpty();

        var created = await _customerService.GetCustomerByEmailAsync(request.Email);
        created.Should().NotBeNull();
        created.FirstName.Should().Be("Api");
    }

    [Test]
    public async Task RegisterTwiceShouldSucceedIndependently()
    {
        var first = ExtractSuccess<RegisterResponse>(await _authController.Register(NewRegistration()));
        var second = ExtractSuccess<RegisterResponse>(await _authController.Register(NewRegistration()));

        first.AccessToken.Should().NotBeNullOrEmpty();
        second.AccessToken.Should().NotBeNullOrEmpty();
        second.AccessToken.Should().NotBe(first.AccessToken);
    }

    [Test]
    public async Task RegisterWithExistingEmailShouldReturnBadRequest()
    {
        var result = await _authController.Register(new RegisterRequest
        {
            Email = NopTestsDefaults.AdminEmail,
            Password = "P@ssw0rd1"
        });

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion
}
