using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Api.Mobile.Models;
using Nop.Plugin.Api.Mobile.Models.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Media;
using Nop.Services.Seo;

namespace Nop.Plugin.Api.Mobile.Factories;

public class CatalogModelFactory : ICatalogModelFactory
{
    #region Fields

    protected readonly IWorkContext _workContext;
    protected readonly IStoreContext _storeContext;
    protected readonly IPictureService _pictureService;
    protected readonly IPriceCalculationService _priceCalculationService;
    protected readonly IPriceFormatter _priceFormatter;
    protected readonly IUrlRecordService _urlRecordService;

    #endregion

    #region Ctor

    public CatalogModelFactory(IWorkContext workContext,
        IStoreContext storeContext,
        IPictureService pictureService,
        IPriceCalculationService priceCalculationService,
        IPriceFormatter priceFormatter,
        IUrlRecordService urlRecordService)
    {
        _workContext = workContext;
        _storeContext = storeContext;
        _pictureService = pictureService;
        _priceCalculationService = priceCalculationService;
        _priceFormatter = priceFormatter;
        _urlRecordService = urlRecordService;
    }

    #endregion

    #region Methods

    public async Task<CategoryModel> PrepareCategoryModelAsync(Category category)
    {
        return new CategoryModel
        {
            Id = category.Id,
            ParentCategoryId = category.ParentCategoryId,
            Name = category.Name,
            Description = category.Description,
            SeName = await _urlRecordService.GetSeNameAsync(category),
            PictureUrl = await _pictureService.GetPictureUrlAsync(category.PictureId),
            DisplayOrder = category.DisplayOrder
        };
    }

    public async Task<IList<CategoryTreeModel>> PrepareCategoryTreeAsync(IList<Category> categories)
    {
        var categoriesByParent = categories.ToLookup(category => category.ParentCategoryId);
        return await BuildCategoryNodesAsync(0, categoriesByParent);
    }

    protected virtual async Task<IList<CategoryTreeModel>> BuildCategoryNodesAsync(int parentCategoryId, ILookup<int, Category> categoriesByParent)
    {
        var nodes = new List<CategoryTreeModel>();
        foreach (var category in categoriesByParent[parentCategoryId].OrderBy(category => category.DisplayOrder))
        {
            nodes.Add(new CategoryTreeModel
            {
                Id = category.Id,
                ParentCategoryId = category.ParentCategoryId,
                Name = category.Name,
                SeName = await _urlRecordService.GetSeNameAsync(category),
                PictureUrl = await _pictureService.GetPictureUrlAsync(category.PictureId),
                DisplayOrder = category.DisplayOrder,
                Children = await BuildCategoryNodesAsync(category.Id, categoriesByParent)
            });
        }

        return nodes;
    }

    public async Task<ManufacturerModel> PrepareManufacturerModelAsync(Manufacturer manufacturer)
    {
        return new ManufacturerModel
        {
            Id = manufacturer.Id,
            Name = manufacturer.Name,
            Description = manufacturer.Description,
            SeName = await _urlRecordService.GetSeNameAsync(manufacturer),
            PictureUrl = await _pictureService.GetPictureUrlAsync(manufacturer.PictureId),
            DisplayOrder = manufacturer.DisplayOrder
        };
    }

    public async Task<ProductOverviewModel> PrepareProductOverviewModelAsync(Product product)
    {
        var (finalPrice, priceFormatted) = await PreparePriceAsync(product);
        var pictures = await _pictureService.GetPicturesByProductIdAsync(product.Id, 1);
        var pictureUrl = pictures.Any()
            ? await _pictureService.GetPictureUrlAsync(pictures.First().Id)
            : await _pictureService.GetPictureUrlAsync(0);

        return new ProductOverviewModel
        {
            Id = product.Id,
            Name = product.Name,
            ShortDescription = product.ShortDescription,
            Sku = product.Sku,
            SeName = await _urlRecordService.GetSeNameAsync(product),
            Price = finalPrice,
            PriceFormatted = priceFormatted,
            PictureUrl = pictureUrl,
            MarkAsNew = product.MarkAsNew
        };
    }

    public async Task<ProductDetailsModel> PrepareProductDetailsModelAsync(Product product)
    {
        var (finalPrice, priceFormatted) = await PreparePriceAsync(product);

        var pictureUrls = new List<string>();
        foreach (var picture in await _pictureService.GetPicturesByProductIdAsync(product.Id))
            pictureUrls.Add(await _pictureService.GetPictureUrlAsync(picture.Id));

        return new ProductDetailsModel
        {
            Id = product.Id,
            Name = product.Name,
            ShortDescription = product.ShortDescription,
            FullDescription = product.FullDescription,
            Sku = product.Sku,
            Gtin = product.Gtin,
            SeName = await _urlRecordService.GetSeNameAsync(product),
            Price = finalPrice,
            PriceFormatted = priceFormatted,
            OldPrice = product.OldPrice,
            OldPriceFormatted = product.OldPrice > decimal.Zero
                ? await _priceFormatter.FormatPriceAsync(product.OldPrice)
                : null,
            MarkAsNew = product.MarkAsNew,
            PictureUrls = pictureUrls
        };
    }

    public async Task<IList<ProductOverviewModel>> PrepareProductOverviewModelsAsync(IEnumerable<Product> products)
    {
        var models = new List<ProductOverviewModel>();
        foreach (var product in products)
            models.Add(await PrepareProductOverviewModelAsync(product));

        return models;
    }

    public async Task<PagedResponse<ProductOverviewModel>> PrepareProductPagedResponseAsync(IPagedList<Product> products)
    {
        var items = await PrepareProductOverviewModelsAsync(products);
        return PagedResponse<ProductOverviewModel>.Create(products, items);
    }

    #endregion

    #region Utilities

    protected virtual async Task<(decimal finalPrice, string priceFormatted)> PreparePriceAsync(Product product)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();

        var (_, finalPrice, _, _) = await _priceCalculationService.GetFinalPriceAsync(product, customer, store);
        var priceFormatted = await _priceFormatter.FormatPriceAsync(finalPrice);

        return (finalPrice, priceFormatted);
    }

    #endregion
}
