using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Api.Mobile.Models;
using Nop.Plugin.Api.Mobile.Models.Catalog;

namespace Nop.Plugin.Api.Mobile.Factories;

public interface ICatalogModelFactory
{
    Task<CategoryModel> PrepareCategoryModelAsync(Category category);

    Task<ManufacturerModel> PrepareManufacturerModelAsync(Manufacturer manufacturer);

    Task<ProductOverviewModel> PrepareProductOverviewModelAsync(Product product);

    Task<ProductDetailsModel> PrepareProductDetailsModelAsync(Product product);

    Task<IList<ProductOverviewModel>> PrepareProductOverviewModelsAsync(IEnumerable<Product> products);

    Task<PagedResponse<ProductOverviewModel>> PrepareProductPagedResponseAsync(IPagedList<Product> products);
}
