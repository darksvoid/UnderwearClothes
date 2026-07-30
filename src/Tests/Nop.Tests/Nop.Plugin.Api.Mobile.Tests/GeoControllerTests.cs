using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Common;
using Nop.Plugin.Api.Mobile.Controllers;
using Nop.Plugin.Api.Mobile.Models;
using Nop.Plugin.Api.Mobile.Models.Geo;
using Nop.Services.Directory;
using Nop.Tests.Nop.Services.Tests;
using NUnit.Framework;

namespace Nop.Tests.Nop.Plugin.Api.Mobile.Tests;

[TestFixture]
public class GeoControllerTests : ServiceTest
{
    #region Fields

    private ICountryService _countryService;
    private IStateProvinceService _stateProvinceService;
    private GeoController _geoController;

    #endregion

    #region SetUp

    [OneTimeSetUp]
    public void SetUp()
    {
        _countryService = GetService<ICountryService>();
        _stateProvinceService = GetService<IStateProvinceService>();
        _geoController = new GeoController(_stateProvinceService, GetService<AddressSettings>());
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

    #endregion

    #region Tests

    [Test]
    public async Task GetStatesForCountryWithRegionsShouldReturnList()
    {
        var countries = await _countryService.GetAllCountriesAsync();

        int countryWithStates = 0;
        foreach (var country in countries)
        {
            var states = await _stateProvinceService.GetStateProvincesByCountryIdAsync(country.Id);
            if (states.Any())
            {
                countryWithStates = country.Id;
                break;
            }
        }

        if (countryWithStates == 0)
            Assert.Ignore("No sample country with states was found.");

        var data = ExtractSuccess<List<StateProvinceModel>>(await _geoController.GetStates(countryWithStates));

        data.Should().NotBeEmpty();
        data.Should().OnlyContain(state => state.Id > 0 && !string.IsNullOrEmpty(state.Name));
    }

    [Test]
    public async Task GetStatesForUnknownCountryShouldReturnEmptyList()
    {
        var data = ExtractSuccess<List<StateProvinceModel>>(await _geoController.GetStates(int.MaxValue));

        data.Should().BeEmpty();
    }

    #endregion
}
