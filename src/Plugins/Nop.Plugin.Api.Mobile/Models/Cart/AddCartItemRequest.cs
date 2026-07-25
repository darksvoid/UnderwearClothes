using System.ComponentModel.DataAnnotations;

namespace Nop.Plugin.Api.Mobile.Models.Cart;

public class AddCartItemRequest
{
    [Required]
    public int ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; } = 1;
}
