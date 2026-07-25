namespace Nop.Plugin.Api.Mobile.Models.Cart;

public class WishlistModel
{
    public IList<CartItemModel> Items { get; set; } = new List<CartItemModel>();

    public int TotalItems { get; set; }
}
