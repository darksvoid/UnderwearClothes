using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Api.Mobile.Factories;
using Nop.Plugin.Api.Mobile.Models;
using Nop.Plugin.Api.Mobile.Models.Catalog;
using Nop.Services.Catalog;

namespace Nop.Plugin.Api.Mobile.Controllers;

/// <summary>
/// Read-only catalog categories.
/// </summary>
public class CategoriesController : BaseApiController
{
    #region Fields

    protected readonly ICategoryService _categoryService;
    protected readonly IProductService _productService;
    protected readonly IStoreContext _storeContext;
    protected readonly ICatalogModelFactory _catalogModelFactory;

    #endregion

    #region Ctor

    public CategoriesController(ICategoryService categoryService,
        IProductService productService,
        IStoreContext storeContext,
        ICatalogModelFactory catalogModelFactory)
    {
        _categoryService = categoryService;
        _productService = productService;
        _storeContext = storeContext;
        _catalogModelFactory = catalogModelFactory;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Returns all published categories of the store.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IList<CategoryModel>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        var categories = await _categoryService.GetAllCategoriesAsync(store.Id);

        var models = new List<CategoryModel>();
        foreach (var category in categories)
            models.Add(await _catalogModelFactory.PrepareCategoryModelAsync(category));

        return Success(models);
    }

    /// <summary>
    /// Returns the categories of the store as a nested tree.
    /// </summary>
    [HttpGet("tree")]
    [ProducesResponseType(typeof(ApiResponse<IList<CategoryTreeModel>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTree()
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        var categories = await _categoryService.GetAllCategoriesAsync(store.Id);

        return Success(await _catalogModelFactory.PrepareCategoryTreeAsync(categories));
    }

    /// <summary>
    /// Returns a single category by identifier.
    /// </summary>
    /// <response code="404">The category was not found.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CategoryModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(int id)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id);
        if (category is null || category.Deleted || !category.Published)
            return NotFoundError("Category not found.");

        return Success(await _catalogModelFactory.PrepareCategoryModelAsync(category));
    }

    /// <summary>
    /// Returns the products of a category (paged).
    /// </summary>
    [HttpGet("{id:int}/products")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<ProductOverviewModel>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducts(int id, [FromQuery] int pageIndex = 0, [FromQuery] int pageSize = ApiMobileDefaults.DefaultPageSize)
    {
        var store = await _storeContext.GetCurrentStoreAsync();

        var products = await _productService.SearchProductsAsync(
            pageIndex: NormalizePageIndex(pageIndex),
            pageSize: NormalizePageSize(pageSize),
            categoryIds: new List<int> { id },
            storeId: store.Id,
            visibleIndividuallyOnly: true);

        return Success(await _catalogModelFactory.PrepareProductPagedResponseAsync(products));
    }

    #endregion
}
