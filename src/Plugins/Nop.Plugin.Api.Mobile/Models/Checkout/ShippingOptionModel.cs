namespace Nop.Plugin.Api.Mobile.Models.Checkout;

public class ShippingOptionModel
{
    public string Name { get; set; }

    public string Description { get; set; }

    public decimal Rate { get; set; }

    public string RateFormatted { get; set; }

    public string SystemName { get; set; }
}
