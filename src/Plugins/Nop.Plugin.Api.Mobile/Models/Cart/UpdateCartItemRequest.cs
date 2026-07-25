using System.ComponentModel.DataAnnotations;

namespace Nop.Plugin.Api.Mobile.Models.Cart;

public class UpdateCartItemRequest
{
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}
