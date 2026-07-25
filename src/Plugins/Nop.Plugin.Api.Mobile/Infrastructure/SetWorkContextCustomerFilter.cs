using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.JsonWebTokens;
using Nop.Core;
using Nop.Services.Customers;

namespace Nop.Plugin.Api.Mobile.Infrastructure;

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
