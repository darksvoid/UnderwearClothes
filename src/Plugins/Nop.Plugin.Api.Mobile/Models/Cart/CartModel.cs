namespace Nop.Plugin.Api.Mobile.Models.Cart;

public class CartModel
{
    public IList<CartItemModel> Items { get; set; } = new List<CartItemModel>();

    public int TotalItems { get; set; }

    public decimal SubTotal { get; set; }

    public string SubTotalFormatted { get; set; }
}
