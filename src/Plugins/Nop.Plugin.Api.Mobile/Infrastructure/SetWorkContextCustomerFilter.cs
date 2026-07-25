using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.JsonWebTokens;
using Nop.Core;
using Nop.Services.Customers;

namespace Nop.Plugin.Api.Mobile.Infrastructure;

/// <summary>
/// When a request is authenticated via a mobile API token, resolves the corresponding customer
/// and sets it as the current customer on the work context, so downstream services and factories
/// (orders, cart, prices) operate on behalf of the authenticated customer.
/// </summary>
public class SetWorkContextCustomerFilter : IAsyncActionFilter
{
    #region Fields

    protected readonly IWorkContext _workContext;
    protected readonly ICustomerService _customerService;

    #endregion

    #region Ctor

    public SetWorkContextCustomerFilter(IWorkContext workContext,
        ICustomerService customerService)
    {
        _workContext = workContext;
        _customerService = customerService;
    }

    #endregion

    #region Methods

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var user = context.HttpContext.User;

        if (user?.Identity?.IsAuthenticated == true)
        {
            var sub = user.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (int.TryParse(sub, out var customerId))
            {
                var customer = await _customerService.GetCustomerByIdAsync(customerId);
                if (customer is { Deleted: false, Active: true })
                    await _workContext.SetCurrentCustomerAsync(customer);
            }
        }

        await next();
    }

    #endregion
}
