namespace Nop.Plugin.Api.Mobile.Models.Catalog;

public class ProductDetailsModel
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string ShortDescription { get; set; }

    public string FullDescription { get; set; }

    public string Sku { get; set; }

    public string Gtin { get; set; }

    public string SeName { get; set; }

    public decimal Price { get; set; }

    public string PriceFormatted { get; set; }

    public decimal OldPrice { get; set; }

    public string OldPriceFormatted { get; set; }

    public bool MarkAsNew { get; set; }

    public IList<string> PictureUrls { get; set; } = new List<string>();
}
