using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Api.Mobile;
using Nop.Plugin.Api.Mobile.Controllers;
using Nop.Plugin.Api.Mobile.Factories;
using Nop.Plugin.Api.Mobile.Models;
using Nop.Plugin.Api.Mobile.Models.Catalog;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Media;
using Nop.Services.Seo;
using Nop.Tests.Nop.Services.Tests;
using NUnit.Framework;

namespace Nop.Tests.Nop.Plugin.Api.Mobile.Tests;

[TestFixture]
public class CatalogControllersTests : ServiceTest
{
    #region Fields

    private ICategoryService _categoryService;
    private IManufacturerService _manufacturerService;
    private IProductService _productService;
    private IStoreContext _storeContext;
    private ICatalogModelFactory _catalogModelFactory;

    private CategoriesController _categoriesController;
    private ManufacturersController _manufacturersController;
    private ProductsController _productsController;

    #endregion

    #region SetUp

    [OneTimeSetUp]
    public void SetUp()
    {
        _categoryService = GetService<ICategoryService>();
        _manufacturerService = GetService<IManufacturerService>();
        _productService = GetService<IProductService>();
        _storeContext = GetService<IStoreContext>();

        _catalogModelFactory = new CatalogModelFactory(
            GetService<IWorkContext>(),
            _storeContext,
            GetService<IPictureService>(),
            GetService<IPriceCalculationService>(),
            GetService<IPriceFormatter>(),
            GetService<IUrlRecordService>(),
            _productService,
            GetService<IProductReviewService>(),
            GetService<ISpecificationAttributeService>(),
            GetService<ICustomerService>());

        _categoriesController = new CategoriesController(_categoryService, _productService, _storeContext, _catalogModelFactory);
        _manufacturersController = new ManufacturersController(_manufacturerService, _productService, _storeContext, _catalogModelFactory);
        _productsController = new ProductsController(_productService, _storeContext, _catalogModelFactory);
    }

    #endregion

    #region Utilities

    private static T ExtractSuccess<T>(IActionResult result)
    {
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();

        var response = okResult.Value as ApiResponse<T>;
        response.Should().NotBeNull();
        response.Success.Should().BeTrue();
        response.Error.Should().BeNull();

        return response.Data;
    }

    #endregion

    #region Categories

    [Test]
    public async Task GetAllCategoriesShouldReturnSeededCategories()
    {
        var result = await _categoriesController.GetAll();

        var data = ExtractSuccess<List<CategoryModel>>(result);
        data.Should().NotBeEmpty();
        data.Should().OnlyContain(category => category.Id > 0);
    }

    [Test]
    public async Task GetCategoryByIdShouldReturnCategory()
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        var existing = (await _categoryService.GetAllCategoriesAsync(store.Id)).First();

        var result = await _categoriesController.Get(existing.Id);

        var data = ExtractSuccess<CategoryModel>(result);
        data.Id.Should().Be(existing.Id);
        data.Name.Should().Be(existing.Name);
    }

    [Test]
    public async Task GetCategoryByUnknownIdShouldReturnNotFound()
    {
        var result = await _categoriesController.Get(int.MaxValue);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Test]
    public async Task GetCategoryTreeShouldBeRootedAndComplete()
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        var flatCount = (await _categoryService.GetAllCategoriesAsync(store.Id)).Count;

        var result = await _categoriesController.GetTree();

        var tree = ExtractSuccess<IList<CategoryTreeModel>>(result);
        tree.Should().NotBeEmpty();
        tree.Should().OnlyContain(node => node.ParentCategoryId == 0);
        CountNodes(tree).Should().Be(flatCount);
    }

    [Test]
    public async Task GetCategoryTreeShouldContainNestedChildren()
    {
        var result = await _categoriesController.GetTree();

        var tree = ExtractSuccess<IList<CategoryTreeModel>>(result);
        tree.Any(node => node.Children.Any()).Should().BeTrue();
    }

    private static int CountNodes(IEnumerable<CategoryTreeModel> nodes)
    {
        return nodes.Sum(node => 1 + CountNodes(node.Children));
    }

    [Test]
    public async Task GetCategoryProductsShouldReturnPagedResponse()
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        var existing = (await _categoryService.GetAllCategoriesAsync(store.Id)).First();

        var result = await _categoriesController.GetProducts(existing.Id);

        var data = ExtractSuccess<PagedResponse<ProductOverviewModel>>(result);
        data.Should().NotBeNull();
        data.Items.Should().NotBeNull();
    }

    #endregion

    #region Manufacturers

    [Test]
    public async Task GetAllManufacturersShouldReturnSeededManufacturers()
    {
        var result = await _manufacturersController.GetAll();

        var data = ExtractSuccess<PagedResponse<ManufacturerModel>>(result);
        data.TotalCount.Should().BeGreaterThan(0);
        data.Items.Should().NotBeEmpty();
    }

    [Test]
    public async Task GetManufacturerByIdShouldReturnManufacturer()
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        var existing = (await _manufacturerService.GetAllManufacturersAsync(storeId: store.Id)).First();

        var result = await _manufacturersController.Get(existing.Id);

        var data = ExtractSuccess<ManufacturerModel>(result);
        data.Id.Should().Be(existing.Id);
        data.Name.Should().Be(existing.Name);
    }

    [Test]
    public async Task GetManufacturerByUnknownIdShouldReturnNotFound()
    {
        var result = await _manufacturersController.Get(int.MaxValue);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region Products

    [Test]
    public async Task SearchProductsShouldReturnSeededProducts()
    {
        var result = await _productsController.Search();

        var data = ExtractSuccess<PagedResponse<ProductOverviewModel>>(result);
        data.TotalCount.Should().BeGreaterThan(0);
        data.Items.Should().NotBeEmpty();
    }

    [Test]
    public async Task SearchProductsShouldClampPageSizeToMaximum()
    {
        var result = await _productsController.Search(pageSize: 1000);

        var data = ExtractSuccess<PagedResponse<ProductOverviewModel>>(result);
        data.PageSize.Should().Be(ApiMobileDefaults.MaxPageSize);
    }

    [Test]
    public async Task GetProductByIdShouldReturnDetails()
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        var existing = (await _productService.SearchProductsAsync(
            pageSize: 1, storeId: store.Id, visibleIndividuallyOnly: true)).First();

        var result = await _productsController.Get(existing.Id);

        var data = ExtractSuccess<ProductDetailsModel>(result);
        data.Id.Should().Be(existing.Id);
        data.Name.Should().Be(existing.Name);
    }

    [Test]
    public async Task GetProductByUnknownIdShouldReturnNotFound()
    {
        var result = await _productsController.Get(int.MaxValue);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Test]
    public async Task SearchSortedByPriceAscendingShouldOrderResults()
    {
        var result = await _productsController.Search(orderBy: (int)ProductSortingEnum.PriceAsc, pageSize: 50);

        var data = ExtractSuccess<PagedResponse<ProductOverviewModel>>(result);
        data.Items.Select(item => item.Price).Should().BeInAscendingOrder();
    }

    [Test]
    public async Task SearchWithMaxPriceShouldNotExceedUnfilteredCount()
    {
        var unfiltered = ExtractSuccess<PagedResponse<ProductOverviewModel>>(await _productsController.Search(pageSize: 1));
        var filtered = ExtractSuccess<PagedResponse<ProductOverviewModel>>(await _productsController.Search(priceMax: 10m, pageSize: 1));

        filtered.TotalCount.Should().BeLessThanOrEqualTo(unfiltered.TotalCount);
    }

    [Test]
    public async Task ProductDetailsShouldExposeReviews()
    {
        var reviewService = GetService<IProductReviewService>();
        var anyReview = (await reviewService.GetAllProductReviewsAsync(approved: true, pageSize: 1)).FirstOrDefault();
        if (anyReview is null)
            Assert.Ignore("No sample product reviews were seeded.");

        var product = await _productService.GetProductByIdAsync(anyReview.ProductId);
        var details = await _catalogModelFactory.PrepareProductDetailsModelAsync(product);

        details.Reviews.Should().Contain(review => review.Id == anyReview.Id);
        details.TotalReviews.Should().BeGreaterThan(0);
    }

    [Test]
    public async Task ProductDetailsShouldExposeSpecifications()
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        var specificationAttributeService = GetService<ISpecificationAttributeService>();
        var products = await _productService.SearchProductsAsync(pageSize: 100, storeId: store.Id, visibleIndividuallyOnly: true);

        Product productWithSpecs = null;
        foreach (var product in products)
        {
            var specs = await specificationAttributeService.GetProductSpecificationAttributesAsync(product.Id, showOnProductPage: true);
            if (specs.Any())
            {
                productWithSpecs = product;
                break;
            }
        }

        if (productWithSpecs is null)
            Assert.Ignore("No sample product with specifications was found.");

        var details = await _catalogModelFactory.PrepareProductDetailsModelAsync(productWithSpecs);

        details.Specifications.Should().NotBeEmpty();
        details.Specifications.Should().OnlyContain(spec => !string.IsNullOrEmpty(spec.Name));
    }

    #endregion
}
