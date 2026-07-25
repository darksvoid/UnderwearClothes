using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Plugin.Api.Mobile.Factories;
using Nop.Plugin.Api.Mobile.Models;
using Nop.Plugin.Api.Mobile.Models.Customers;
using Nop.Services.Common;
using Nop.Services.Customers;

namespace Nop.Plugin.Api.Mobile.Controllers;

/// <summary>
/// Authenticated customer account: profile and addresses (read-only).
/// </summary>
[Authorize(AuthenticationSchemes = ApiMobileDefaults.AuthenticationScheme)]
public class CustomerController : BaseApiController
{
    #region Fields

    protected readonly IWorkContext _workContext;
    protected readonly ICustomerService _customerService;
    protected readonly IAddressService _addressService;
    protected readonly ICustomerModelFactory _customerModelFactory;

    #endregion

    #region Ctor

    public CustomerController(IWorkContext workContext,
        ICustomerService customerService,
        IAddressService addressService,
        ICustomerModelFactory customerModelFactory)
    {
        _workContext = workContext;
        _customerService = customerService;
        _addressService = addressService;
        _customerModelFactory = customerModelFactory;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Returns the profile of the authenticated customer.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<CustomerProfileModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProfile()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        return Success(await _customerModelFactory.PrepareCustomerProfileModelAsync(customer));
    }

    /// <summary>
    /// Returns the addresses of the authenticated customer.
    /// </summary>
    [HttpGet("addresses")]
    [ProducesResponseType(typeof(ApiResponse<IList<AddressModel>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAddresses()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var addresses = await _customerService.GetAddressesByCustomerIdAsync(customer.Id);

        var models = new List<AddressModel>();
        foreach (var address in addresses)
            models.Add(await _customerModelFactory.PrepareAddressModelAsync(address));

        return Success(models);
    }

    /// <summary>
    /// Returns a single address of the authenticated customer.
    /// </summary>
    /// <response code="404">The address was not found for this customer.</response>
    [HttpGet("addresses/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<AddressModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAddress(int id)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var addresses = await _customerService.GetAddressesByCustomerIdAsync(customer.Id);

        var address = addresses.FirstOrDefault(a => a.Id == id);
        if (address is null)
            return NotFoundError("Address not found.");

        return Success(await _customerModelFactory.PrepareAddressModelAsync(address));
    }

    /// <summary>
    /// Creates a new address for the authenticated customer.
    /// </summary>
    /// <response code="400">The address is invalid or incomplete.</response>
    [HttpPost("addresses")]
    [ProducesResponseType(typeof(ApiResponse<AddressModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAddress([FromBody] AddressRequest request)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();

        var address = new Address { CreatedOnUtc = DateTime.UtcNow };
        ApplyRequest(address, request);

        if (!await _addressService.IsAddressValidAsync(address))
            return BadRequest(ApiResponse.Fail("invalid_address", "The address is invalid or incomplete."));

        await _addressService.InsertAddressAsync(address);
        await _customerService.InsertCustomerAddressAsync(customer, address);

        return Success(await _customerModelFactory.PrepareAddressModelAsync(address));
    }

    /// <summary>
    /// Updates an existing address of the authenticated customer.
    /// </summary>
    /// <response code="400">The address is invalid or incomplete.</response>
    /// <response code="404">The address was not found for this customer.</response>
    [HttpPut("addresses/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<AddressModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAddress(int id, [FromBody] AddressRequest request)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var addresses = await _customerService.GetAddressesByCustomerIdAsync(customer.Id);

        var address = addresses.FirstOrDefault(a => a.Id == id);
        if (address is null)
            return NotFoundError("Address not found.");

        ApplyRequest(address, request);

        if (!await _addressService.IsAddressValidAsync(address))
            return BadRequest(ApiResponse.Fail("invalid_address", "The address is invalid or incomplete."));

        await _addressService.UpdateAddressAsync(address);

        return Success(await _customerModelFactory.PrepareAddressModelAsync(address));
    }

    /// <summary>
    /// Deletes an address of the authenticated customer.
    /// </summary>
    /// <response code="404">The address was not found for this customer.</response>
    [HttpDelete("addresses/{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAddress(int id)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var addresses = await _customerService.GetAddressesByCustomerIdAsync(customer.Id);

        var address = addresses.FirstOrDefault(a => a.Id == id);
        if (address is null)
            return NotFoundError("Address not found.");

        await _customerService.RemoveCustomerAddressAsync(customer, address);
        await _addressService.DeleteAddressAsync(address);

        return Success(new { message = "Address removed." });
    }

    #endregion

    #region Utilities

    private static void ApplyRequest(Address address, AddressRequest request)
    {
        address.FirstName = request.FirstName;
        address.LastName = request.LastName;
        address.Email = request.Email;
        address.Company = request.Company;
        address.CountryId = request.CountryId;
        address.StateProvinceId = request.StateProvinceId;
        address.County = request.County;
        address.City = request.City;
        address.Address1 = request.Address1;
        address.Address2 = request.Address2;
        address.ZipPostalCode = request.ZipPostalCode;
        address.PhoneNumber = request.PhoneNumber;
        address.FaxNumber = request.FaxNumber;
    }

    #endregion
}
