using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Topics;
using Nop.Plugin.Api.Mobile.Models;
using Nop.Plugin.Api.Mobile.Models.Cms;
using Nop.Services.Topics;

namespace Nop.Plugin.Api.Mobile.Controllers;

/// <summary>
/// Read-only CMS pages (topics): About us, Privacy notice, Shipping, Terms, etc.
/// </summary>
public class TopicsController : BaseApiController
{
    #region Fields

    protected readonly ITopicService _topicService;
    protected readonly IStoreContext _storeContext;

    #endregion

    #region Ctor

    public TopicsController(ITopicService topicService, IStoreContext storeContext)
    {
        _topicService = topicService;
        _storeContext = storeContext;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Returns the published, publicly accessible CMS pages (without the body).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IList<TopicModel>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        var topics = await _topicService.GetAllTopicsAsync(store.Id);

        var models = topics
            .Where(topic => !topic.IsPasswordProtected)
            .Select(topic => Map(topic, includeBody: false))
            .ToList();

        return Success(models);
    }

    /// <summary>
    /// Returns a single CMS page by its system name, including the body.
    /// </summary>
    /// <response code="404">The page was not found or is not publicly accessible.</response>
    [HttpGet("{systemName}")]
    [ProducesResponseType(typeof(ApiResponse<TopicModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(string systemName)
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        var topic = await _topicService.GetTopicBySystemNameAsync(systemName, store.Id);

        if (topic is null || !topic.Published || topic.IsPasswordProtected)
            return NotFoundError("Topic not found.");

        return Success(Map(topic, includeBody: true));
    }

    #endregion

    #region Utilities

    private static TopicModel Map(Topic topic, bool includeBody)
    {
        return new TopicModel
        {
            Id = topic.Id,
            SystemName = topic.SystemName,
            Title = topic.Title,
            Body = includeBody ? topic.Body : null,
            MetaTitle = topic.MetaTitle,
            MetaKeywords = topic.MetaKeywords,
            MetaDescription = topic.MetaDescription
        };
    }

    #endregion
}
