using Nop.Core;

namespace Nop.Plugin.Api.Mobile.Models;

public class PagedResponse<T>
{
    public IList<T> Items { get; set; } = new List<T>();

    public int PageIndex { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public int TotalPages { get; set; }

    public PagedResponse()
    {
    }

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
