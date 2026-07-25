namespace Nop.Plugin.Api.Mobile.Models.Catalog;

public class ProductOverviewModel
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string ShortDescription { get; set; }

    public string Sku { get; set; }

    public string SeName { get; set; }

    public decimal Price { get; set; }

    public string PriceFormatted { get; set; }

    public string PictureUrl { get; set; }

    public bool MarkAsNew { get; set; }
}
