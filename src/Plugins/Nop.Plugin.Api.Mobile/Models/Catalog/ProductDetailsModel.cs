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

    public double AverageRating { get; set; }

    public int TotalReviews { get; set; }

    public IList<ProductSpecificationModel> Specifications { get; set; } = new List<ProductSpecificationModel>();

    public IList<ProductReviewModel> Reviews { get; set; } = new List<ProductReviewModel>();

    public IList<ProductOverviewModel> RelatedProducts { get; set; } = new List<ProductOverviewModel>();
}
