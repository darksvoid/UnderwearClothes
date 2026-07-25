namespace Nop.Plugin.Api.Mobile.Models.Checkout;

public class CheckoutDataModel
{
    public bool RequiresShipping { get; set; }

    public decimal SubTotal { get; set; }

    public string SubTotalFormatted { get; set; }

    public IList<ShippingOptionModel> ShippingOptions { get; set; } = new List<ShippingOptionModel>();

    public IList<PaymentMethodModel> PaymentMethods { get; set; } = new List<PaymentMethodModel>();
}
