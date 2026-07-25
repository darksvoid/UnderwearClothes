using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Api.Mobile.Factories;
using Nop.Plugin.Api.Mobile.Models;
using Nop.Plugin.Api.Mobile.Models.Catalog;
using Nop.Services.Catalog;

namespace Nop.Plugin.Api.Mobile.Controllers;

/// <summary>
/// Read-only catalog products: search, listing and details.
/// </summary>
public class ProductsController : BaseApiController
{
    #region Fields

    protected readonly IProductService _productService;
    protected readonly IStoreContext _storeContext;
    protected readonly ICatalogModelFactory _catalogModelFactory;

    #endregion

    #region Ctor

    public ProductsController(IProductService productService,
        IStoreContext storeContext,
        ICatalogModelFactory catalogModelFactory)
    {
        _productService = productService;
        _storeContext = storeContext;
        _catalogModelFactory = catalogModelFactory;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Searches products with optional keyword, category and manufacturer filters (paged).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<ProductOverviewModel>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] string keywords = null,
        [FromQuery] int categoryId = 0,
        [FromQuery] int manufacturerId = 0,
        [FromQuery] decimal? priceMin = null,
        [FromQuery] decimal? priceMax = null,
        [FromQuery] int orderBy = 0,
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = ApiMobileDefaults.DefaultPageSize)
    {
        var store = await _storeContext.GetCurrentStoreAsync();

        var sorting = Enum.IsDefined(typeof(ProductSortingEnum), orderBy)
            ? (ProductSortingEnum)orderBy
            : ProductSortingEnum.Position;

        var products = await _productService.SearchProductsAsync(
            pageIndex: NormalizePageIndex(pageIndex),
            pageSize: NormalizePageSize(pageSize),
            categoryIds: categoryId > 0 ? new List<int> { categoryId } : null,
            manufacturerIds: manufacturerId > 0 ? new List<int> { manufacturerId } : null,
            storeId: store.Id,
            priceMin: priceMin,
            priceMax: priceMax,
            keywords: keywords,
            orderBy: sorting,
            visibleIndividuallyOnly: true);

        return Success(await _catalogModelFactory.PrepareProductPagedResponseAsync(products));
    }

    /// <summary>
    /// Returns product details by identifier.
    /// </summary>
    /// <response code="404">The product was not found.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDetailsModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        if (product is null || product.Deleted || !product.Published || !product.VisibleIndividually)
            return NotFoundError("Product not found.");

        return Success(await _catalogModelFactory.PrepareProductDetailsModelAsync(product));
    }

    #endregion
}
