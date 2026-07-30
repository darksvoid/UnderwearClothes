using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Api.Mobile.Controllers;
using Nop.Plugin.Api.Mobile.Factories;
using Nop.Plugin.Api.Mobile.Models;
using Nop.Plugin.Api.Mobile.Models.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Seo;
using Nop.Tests.Nop.Services.Tests;
using NUnit.Framework;

namespace Nop.Tests.Nop.Plugin.Api.Mobile.Tests;

[TestFixture]
public class HomeControllerTests : ServiceTest
{
    #region Fields

    private HomeController _homeController;

    #endregion

    #region SetUp

    [OneTimeSetUp]
    public void SetUp()
    {
        var storeContext = GetService<IStoreContext>();
        var productService = GetService<IProductService>();

        var catalogModelFactory = new CatalogModelFactory(
            GetService<IWorkContext>(),
            storeContext,
            GetService<IPictureService>(),
            GetService<IPriceCalculationService>(),
            GetService<IPriceFormatter>(),
            GetService<IUrlRecordService>(),
            productService,
            GetService<IProductReviewService>(),
            GetService<ISpecificationAttributeService>(),
            GetService<ICustomerService>());

        _homeController = new HomeController(
            productService,
            GetService<ICategoryService>(),
            GetService<IOrderReportService>(),
            storeContext,
            catalogModelFactory);
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

    #region Tests

    [Test]
    public async Task FeaturedShouldReturnList()
    {
        var data = ExtractSuccess<IList<ProductOverviewModel>>(await _homeController.Featured());

        data.Should().NotBeNull();
        data.Should().OnlyContain(product => product.Id > 0);
    }

    [Test]
    public async Task NewShouldReturnList()
    {
        var data = ExtractSuccess<IList<ProductOverviewModel>>(await _homeController.New());

        data.Should().NotBeNull();
        data.Should().OnlyContain(product => product.Id > 0);
    }

    [Test]
    public async Task BestsellersShouldReturnList()
    {
        var data = ExtractSuccess<IList<ProductOverviewModel>>(await _homeController.Bestsellers());

        data.Should().NotBeNull();
        data.Should().OnlyContain(product => product.Id > 0);
    }

    [Test]
    public async Task CategoriesShouldReturnList()
    {
        var data = ExtractSuccess<List<CategoryModel>>(await _homeController.Categories());

        data.Should().NotBeNull();
        data.Should().OnlyContain(category => category.Id > 0);
    }

    #endregion
}
