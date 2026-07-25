namespace Nop.Plugin.Api.Mobile.Models.Orders;

public class OrderItemModel
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public string ProductName { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public string UnitPriceFormatted { get; set; }

    public decimal SubTotal { get; set; }

    public string SubTotalFormatted { get; set; }
}
