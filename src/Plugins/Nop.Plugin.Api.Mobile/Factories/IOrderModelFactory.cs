using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Api.Mobile.Models;
using Nop.Plugin.Api.Mobile.Models.Orders;

namespace Nop.Plugin.Api.Mobile.Factories;

public interface IOrderModelFactory
{
    Task<OrderOverviewModel> PrepareOrderOverviewModelAsync(Order order);

    Task<OrderDetailsModel> PrepareOrderDetailsModelAsync(Order order);

    Task<PagedResponse<OrderOverviewModel>> PrepareOrderPagedResponseAsync(IPagedList<Order> orders);
}
