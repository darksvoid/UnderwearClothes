using System.ComponentModel.DataAnnotations;

namespace Nop.Plugin.Api.Mobile.Models.Checkout;

public class PlaceOrderRequest
{
    [Required]
    public int BillingAddressId { get; set; }

    public int? ShippingAddressId { get; set; }

    public string ShippingOptionName { get; set; }

    public string ShippingRateComputationMethodSystemName { get; set; }

    [Required]
    public string PaymentMethodSystemName { get; set; }
}
