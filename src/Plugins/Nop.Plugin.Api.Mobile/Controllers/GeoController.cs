using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Common;
using Nop.Plugin.Api.Mobile.Models;
using Nop.Plugin.Api.Mobile.Models.Geo;
using Nop.Services.Directory;

namespace Nop.Plugin.Api.Mobile.Controllers;

/// <summary>
/// Read-only geographic reference data for address forms.
/// </summary>
public class GeoController : BaseApiController
{
    #region Fields

    protected readonly IStateProvinceService _stateProvinceService;
    protected readonly AddressSettings _addressSettings;

    #endregion

    #region Ctor

    public GeoController(IStateProvinceService stateProvinceService, AddressSettings addressSettings)
    {
        _stateProvinceService = stateProvinceService;
        _addressSettings = addressSettings;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Returns the states/provinces (regions) of a country. When the country is not specified,
    /// the store's default country is used.
    /// </summary>
    [HttpGet("states")]
    [ProducesResponseType(typeof(ApiResponse<IList<StateProvinceModel>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStates([FromQuery] int? countryId = null)
    {
        var resolvedCountryId = countryId ?? _addressSettings.DefaultCountryId ?? 0;
        if (resolvedCountryId <= 0)
            return Success(new List<StateProvinceModel>());

        var states = await _stateProvinceService.GetStateProvincesByCountryIdAsync(resolvedCountryId);

        var models = states
            .Select(state => new StateProvinceModel
            {
                Id = state.Id,
                Name = state.Name,
                Abbreviation = state.Abbreviation
            })
            .ToList();

        return Success(models);
    }

    #endregion
}
