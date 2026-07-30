using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Api.Mobile.Controllers;
using Nop.Plugin.Api.Mobile.Models;
using Nop.Plugin.Api.Mobile.Models.Cms;
using Nop.Services.Topics;
using Nop.Tests.Nop.Services.Tests;
using NUnit.Framework;

namespace Nop.Tests.Nop.Plugin.Api.Mobile.Tests;

[TestFixture]
public class TopicsControllerTests : ServiceTest
{
    #region Fields

    private ITopicService _topicService;
    private IStoreContext _storeContext;
    private TopicsController _topicsController;

    #endregion

    #region SetUp

    [OneTimeSetUp]
    public void SetUp()
    {
        _topicService = GetService<ITopicService>();
        _storeContext = GetService<IStoreContext>();
        _topicsController = new TopicsController(_topicService, _storeContext);
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
    public async Task GetAllShouldReturnPublishedTopics()
    {
        var data = ExtractSuccess<List<TopicModel>>(await _topicsController.GetAll());

        data.Should().NotBeNull();
        data.Should().OnlyContain(topic => !string.IsNullOrEmpty(topic.SystemName));
    }

    [Test]
    public async Task GetBySystemNameShouldReturnTopicWithBody()
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        var existing = (await _topicService.GetAllTopicsAsync(store.Id))
            .FirstOrDefault(topic => !topic.IsPasswordProtected && !string.IsNullOrEmpty(topic.SystemName));
        if (existing is null)
            Assert.Ignore("No sample topics were seeded.");

        var data = ExtractSuccess<TopicModel>(await _topicsController.Get(existing.SystemName));

        data.SystemName.Should().Be(existing.SystemName);
        data.Body.Should().Be(existing.Body);
    }

    [Test]
    public async Task GetByUnknownSystemNameShouldReturnNotFound()
    {
        var result = await _topicsController.Get("this-topic-does-not-exist");

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion
}
