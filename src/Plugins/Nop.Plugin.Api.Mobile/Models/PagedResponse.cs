using Nop.Core;

namespace Nop.Plugin.Api.Mobile.Models;

/// <summary>
/// Represents a page of items along with pagination metadata
/// </summary>
/// <typeparam name="T">Type of the item</typeparam>
public class PagedResponse<T>
{
    /// <summary>
    /// Gets or sets the items on the current page
    /// </summary>
    public IList<T> Items { get; set; } = new List<T>();

    /// <summary>
    /// Gets or sets the zero-based page index
    /// </summary>
    public int PageIndex { get; set; }

    /// <summary>
    /// Gets or sets the page size
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Gets or sets the total number of items across all pages
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Gets or sets the total number of pages
    /// </summary>
    public int TotalPages { get; set; }

    public PagedResponse()
    {
    }

    /// <summary>
    /// Builds a paged response from a nopCommerce paged list, mapping each source item to the target type
    /// </summary>
    public static PagedResponse<T> Create<TSource>(IPagedList<TSource> source, IList<T> mappedItems)
    {
        return new PagedResponse<T>
        {
            Items = mappedItems,
            PageIndex = source.PageIndex,
            PageSize = source.PageSize,
            TotalCount = source.TotalCount,
            TotalPages = source.TotalPages
        };
    }
}
