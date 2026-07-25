using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Api.Mobile.Models.Customers;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;

namespace Nop.Plugin.Api.Mobile.Factories;

public class CustomerModelFactory : ICustomerModelFactory
{
    #region Fields

    protected readonly ICustomerService _customerService;
    protected readonly ICountryService _countryService;
    protected readonly IStateProvinceService _stateProvinceService;
    protected readonly ILocalizationService _localizationService;

    #endregion

    #region Ctor

    public CustomerModelFactory(ICustomerService customerService,
        ICountryService countryService,
        IStateProvinceService stateProvinceService,
        ILocalizationService localizationService)
    {
        _customerService = customerService;
        _countryService = countryService;
        _stateProvinceService = stateProvinceService;
        _localizationService = localizationService;
    }

    #endregion

    #region Methods

    public async Task<CustomerProfileModel> PrepareCustomerProfileModelAsync(Customer customer)
    {
        return new CustomerProfileModel
        {
            Id = customer.Id,
            Email = customer.Email,
            Username = customer.Username,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            FullName = await _customerService.GetCustomerFullNameAsync(customer),
            Company = customer.Company,
            Phone = customer.Phone,
            Gender = customer.Gender,
            DateOfBirth = customer.DateOfBirth,
            CreatedOnUtc = customer.CreatedOnUtc
        };
    }

    public async Task<AddressModel> PrepareAddressModelAsync(Address address)
    {
        var country = address.CountryId.HasValue
            ? await _countryService.GetCountryByAddressAsync(address)
            : null;

        var stateProvince = address.StateProvinceId.HasValue
            ? await _stateProvinceService.GetStateProvinceByAddressAsync(address)
            : null;

        return new AddressModel
        {
            Id = address.Id,
            FirstName = address.FirstName,
            LastName = address.LastName,
            Email = address.Email,
            Company = address.Company,
            CountryId = address.CountryId,
            CountryName = country is null ? null : await _localizationService.GetLocalizedAsync(country, c => c.Name),
            StateProvinceId = address.StateProvinceId,
            StateProvinceName = stateProvince is null ? null : await _localizationService.GetLocalizedAsync(stateProvince, s => s.Name),
            County = address.County,
            City = address.City,
            Address1 = address.Address1,
            Address2 = address.Address2,
            ZipPostalCode = address.ZipPostalCode,
            PhoneNumber = address.PhoneNumber,
            FaxNumber = address.FaxNumber
        };
    }

    #endregion
}
