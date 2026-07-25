namespace Nop.Plugin.Api.Mobile.Models.Orders;

public class OrderDetailsModel
{
    public int Id { get; set; }

    public string OrderNumber { get; set; }

    public string OrderStatus { get; set; }

    public string PaymentStatus { get; set; }

    public string ShippingStatus { get; set; }

    public decimal OrderTotal { get; set; }

    public string OrderTotalFormatted { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public IList<OrderItemModel> Items { get; set; } = new List<OrderItemModel>();
}
