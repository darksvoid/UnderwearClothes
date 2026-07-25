using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Api.Mobile.Factories;
using Nop.Plugin.Api.Mobile.Models;
using Nop.Plugin.Api.Mobile.Models.Catalog;
using Nop.Services.Catalog;

namespace Nop.Plugin.Api.Mobile.Controllers;

/// <summary>
/// Read-only catalog manufacturers (brands).
/// </summary>
public class ManufacturersController : BaseApiController
{
    #region Fields

    protected readonly IManufacturerService _manufacturerService;
    protected readonly IProductService _productService;
    protected readonly IStoreContext _storeContext;
    protected readonly ICatalogModelFactory _catalogModelFactory;

    #endregion

    #region Ctor

    public ManufacturersController(IManufacturerService manufacturerService,
        IProductService productService,
        IStoreContext storeContext,
        ICatalogModelFactory catalogModelFactory)
    {
        _manufacturerService = manufacturerService;
        _productService = productService;
        _storeContext = storeContext;
        _catalogModelFactory = catalogModelFactory;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Returns the published manufacturers of the store (paged).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<ManufacturerModel>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int pageIndex = 0, [FromQuery] int pageSize = ApiMobileDefaults.DefaultPageSize)
    {
        var store = await _storeContext.GetCurrentStoreAsync();

        var manufacturers = await _manufacturerService.GetAllManufacturersAsync(
            storeId: store.Id,
            pageIndex: NormalizePageIndex(pageIndex),
            pageSize: NormalizePageSize(pageSize));

        var items = new List<ManufacturerModel>();
        foreach (var manufacturer in manufacturers)
            items.Add(await _catalogModelFactory.PrepareManufacturerModelAsync(manufacturer));

        return Success(PagedResponse<ManufacturerModel>.Create(manufacturers, items));
    }

    /// <summary>
    /// Returns a single manufacturer by identifier.
    /// </summary>
    /// <response code="404">The manufacturer was not found.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ManufacturerModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(int id)
    {
        var manufacturer = await _manufacturerService.GetManufacturerByIdAsync(id);
        if (manufacturer is null || manufacturer.Deleted || !manufacturer.Published)
            return NotFoundError("Manufacturer not found.");

        return Success(await _catalogModelFactory.PrepareManufacturerModelAsync(manufacturer));
    }

    /// <summary>
    /// Returns the products of a manufacturer (paged).
    /// </summary>
    [HttpGet("{id:int}/products")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<ProductOverviewModel>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducts(int id, [FromQuery] int pageIndex = 0, [FromQuery] int pageSize = ApiMobileDefaults.DefaultPageSize)
    {
        var store = await _storeContext.GetCurrentStoreAsync();

        var products = await _productService.SearchProductsAsync(
            pageIndex: NormalizePageIndex(pageIndex),
            pageSize: NormalizePageSize(pageSize),
            manufacturerIds: new List<int> { id },
            storeId: store.Id,
            visibleIndividuallyOnly: true);

        return Success(await _catalogModelFactory.PrepareProductPagedResponseAsync(products));
    }

    #endregion
}
