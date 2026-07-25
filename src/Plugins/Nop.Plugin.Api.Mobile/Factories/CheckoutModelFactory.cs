using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Api.Mobile.Models.Checkout;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Shipping;

namespace Nop.Plugin.Api.Mobile.Factories;

public class CheckoutModelFactory : ICheckoutModelFactory
{
    #region Fields

    protected readonly IWorkContext _workContext;
    protected readonly IStoreContext _storeContext;
    protected readonly IShoppingCartService _shoppingCartService;
    protected readonly IOrderTotalCalculationService _orderTotalCalculationService;
    protected readonly IShippingService _shippingService;
    protected readonly IPaymentPluginManager _paymentPluginManager;
    protected readonly ILocalizationService _localizationService;
    protected readonly IPriceFormatter _priceFormatter;

    #endregion

    #region Ctor

    public CheckoutModelFactory(IWorkContext workContext,
        IStoreContext storeContext,
        IShoppingCartService shoppingCartService,
        IOrderTotalCalculationService orderTotalCalculationService,
        IShippingService shippingService,
        IPaymentPluginManager paymentPluginManager,
        ILocalizationService localizationService,
        IPriceFormatter priceFormatter)
    {
        _workContext = workContext;
        _storeContext = storeContext;
        _shoppingCartService = shoppingCartService;
        _orderTotalCalculationService = orderTotalCalculationService;
        _shippingService = shippingService;
        _paymentPluginManager = paymentPluginManager;
        _localizationService = localizationService;
        _priceFormatter = priceFormatter;
    }

    #endregion

    #region Methods

    public async Task<CheckoutDataModel> PrepareCheckoutDataAsync(Customer customer, Address shippingAddress)
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

        var model = new CheckoutDataModel
        {
            RequiresShipping = await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart)
        };

        var (_, _, _, subTotalWithDiscount, _) = await _orderTotalCalculationService.GetShoppingCartSubTotalAsync(cart, true);
        model.SubTotal = subTotalWithDiscount;
        model.SubTotalFormatted = await _priceFormatter.FormatPriceAsync(subTotalWithDiscount);

        if (model.RequiresShipping && shippingAddress != null)
        {
            var response = await _shippingService.GetShippingOptionsAsync(cart, shippingAddress, customer, storeId: store.Id);
            if (response.Success)
            {
                foreach (var option in response.ShippingOptions)
                    model.ShippingOptions.Add(new ShippingOptionModel
                    {
                        Name = option.Name,
                        Description = option.Description,
                        Rate = option.Rate,
                        RateFormatted = await _priceFormatter.FormatPriceAsync(option.Rate),
                        SystemName = option.ShippingRateComputationMethodSystemName
                    });
            }
        }

        var languageId = (await _workContext.GetWorkingLanguageAsync()).Id;
        var paymentMethods = await _paymentPluginManager.LoadActivePluginsAsync(customer, store.Id);
        foreach (var paymentMethod in paymentMethods.Where(pm => pm.PaymentMethodType == PaymentMethodType.Standard))
            model.PaymentMethods.Add(new PaymentMethodModel
            {
                SystemName = paymentMethod.PluginDescriptor.SystemName,
                Name = await _localizationService.GetLocalizedFriendlyNameAsync(paymentMethod, languageId)
            });

        return model;
    }

    #endregion
}
