using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Api.Mobile.Factories;
using Nop.Plugin.Api.Mobile.Models;
using Nop.Plugin.Api.Mobile.Models.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Orders;

namespace Nop.Plugin.Api.Mobile.Controllers;

/// <summary>
/// Read-only home screen data: featured, new and best-selling products and the home categories.
/// </summary>
public class HomeController : BaseApiController
{
    #region Fields

    protected readonly IProductService _productService;
    protected readonly ICategoryService _categoryService;
    protected readonly IOrderReportService _orderReportService;
    protected readonly IStoreContext _storeContext;
    protected readonly ICatalogModelFactory _catalogModelFactory;

    #endregion

    #region Ctor

    public HomeController(IProductService productService,
        ICategoryService categoryService,
        IOrderReportService orderReportService,
        IStoreContext storeContext,
        ICatalogModelFactory catalogModelFactory)
    {
        _productService = productService;
        _categoryService = categoryService;
        _orderReportService = orderReportService;
        _storeContext = storeContext;
        _catalogModelFactory = catalogModelFactory;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Returns the products marked to be shown on the home page.
    /// </summary>
    [HttpGet("featured")]
    [ProducesResponseType(typeof(ApiResponse<IList<ProductOverviewModel>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Featured()
    {
        var products = (await _productService.GetAllProductsDisplayedOnHomepageAsync())
            .Where(product => product.Published && !product.Deleted);

        return Success(await _catalogModelFactory.PrepareProductOverviewModelsAsync(products));
    }

    /// <summary>
    /// Returns the newest products of the store.
    /// </summary>
    [HttpGet("new")]
    [ProducesResponseType(typeof(ApiResponse<IList<ProductOverviewModel>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> New()
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        var products = await _productService.GetProductsMarkedAsNewAsync(store.Id, pageSize: ApiMobileDefaults.DefaultPageSize);

        return Success(await _catalogModelFactory.PrepareProductOverviewModelsAsync(products));
    }

    /// <summary>
    /// Returns the best-selling products of the store based on placed orders.
    /// </summary>
    [HttpGet("bestsellers")]
    [ProducesResponseType(typeof(ApiResponse<IList<ProductOverviewModel>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Bestsellers()
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        var report = await _orderReportService.BestSellersReportAsync(storeId: store.Id, pageSize: ApiMobileDefaults.DefaultPageSize);

        var products = new List<Product>();
        foreach (var line in report)
        {
            var product = await _productService.GetProductByIdAsync(line.ProductId);
            if (product is not null && product.Published && !product.Deleted)
                products.Add(product);
        }

        return Success(await _catalogModelFactory.PrepareProductOverviewModelsAsync(products));
    }

    /// <summary>
    /// Returns the categories marked to be shown on the home page.
    /// </summary>
    [HttpGet("categories")]
    [ProducesResponseType(typeof(ApiResponse<IList<CategoryModel>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Categories()
    {
        var categories = await _categoryService.GetAllCategoriesDisplayedOnHomepageAsync();

        var models = new List<CategoryModel>();
        foreach (var category in categories)
            models.Add(await _catalogModelFactory.PrepareCategoryModelAsync(category));

        return Success(models);
    }

    #endregion
}
