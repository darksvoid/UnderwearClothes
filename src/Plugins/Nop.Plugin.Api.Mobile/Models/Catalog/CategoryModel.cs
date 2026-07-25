namespace Nop.Plugin.Api.Mobile.Models.Catalog;

public class CategoryModel
{
    public int Id { get; set; }

    public int ParentCategoryId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string SeName { get; set; }

    public string PictureUrl { get; set; }

    public int DisplayOrder { get; set; }
}
