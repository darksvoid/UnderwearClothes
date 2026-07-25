using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Api.Mobile.Models.Customers;

namespace Nop.Plugin.Api.Mobile.Factories;

public interface ICustomerModelFactory
{
    Task<CustomerProfileModel> PrepareCustomerProfileModelAsync(Customer customer);

    Task<AddressModel> PrepareAddressModelAsync(Address address);
}
