namespace Nop.Plugin.Api.Mobile.Models.Catalog;

public class ProductReviewModel
{
    public int Id { get; set; }

    public string Title { get; set; }

    public string ReviewText { get; set; }

    public int Rating { get; set; }

    public string ReviewerName { get; set; }

    public DateTime CreatedOnUtc { get; set; }
}
