using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Api.Mobile.Models.Checkout;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Shipping;

namespace Nop.Plugin.Api.Mobile.Services.Checkout;

public class OrderPlacementService : IOrderPlacementService
{
    #region Fields

    protected readonly IStoreContext _storeContext;
    protected readonly IShoppingCartService _shoppingCartService;
    protected readonly ICustomerService _customerService;
    protected readonly IShippingService _shippingService;
    protected readonly IGenericAttributeService _genericAttributeService;
    protected readonly IPaymentPluginManager _paymentPluginManager;
    protected readonly IOrderProcessingService _orderProcessingService;

    #endregion

    #region Ctor

    public OrderPlacementService(IStoreContext storeContext,
        IShoppingCartService shoppingCartService,
        ICustomerService customerService,
        IShippingService shippingService,
        IGenericAttributeService genericAttributeService,
        IPaymentPluginManager paymentPluginManager,
        IOrderProcessingService orderProcessingService)
    {
        _storeContext = storeContext;
        _shoppingCartService = shoppingCartService;
        _customerService = customerService;
        _shippingService = shippingService;
        _genericAttributeService = genericAttributeService;
        _paymentPluginManager = paymentPluginManager;
        _orderProcessingService = orderProcessingService;
    }

    #endregion

    #region Methods

    public async Task<(Order order, IList<string> errors)> PlaceOrderAsync(Customer customer, Address billingAddress, Address shippingAddress, PlaceOrderRequest request)
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

        if (!cart.Any())
            return (null, new List<string> { "The cart is empty." });

        var requiresShipping = await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart);
        if (requiresShipping && shippingAddress is null)
            return (null, new List<string> { "A shipping address is required." });

        customer.BillingAddressId = billingAddress.Id;
        if (requiresShipping)
            customer.ShippingAddressId = shippingAddress.Id;
        await _customerService.UpdateCustomerAsync(customer);

        if (requiresShipping)
        {
            var response = await _shippingService.GetShippingOptionsAsync(cart, shippingAddress, customer, storeId: store.Id);
            var option = response.ShippingOptions.FirstOrDefault(so =>
                !string.IsNullOrEmpty(so.Name) &&
                so.Name.Equals(request.ShippingOptionName, StringComparison.InvariantCultureIgnoreCase) &&
                (so.ShippingRateComputationMethodSystemName ?? string.Empty)
                    .Equals(request.ShippingRateComputationMethodSystemName ?? string.Empty, StringComparison.InvariantCultureIgnoreCase));

            if (option is null)
                return (null, new List<string> { "The selected shipping method is not available." });

            await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.SelectedShippingOptionAttribute, option, store.Id);
        }

        if (!await _paymentPluginManager.IsPluginActiveAsync(request.PaymentMethodSystemName, customer, store.Id))
            return (null, new List<string> { "The selected payment method is not available." });

        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.SelectedPaymentMethodAttribute, request.PaymentMethodSystemName, store.Id);

        var processPaymentRequest = new ProcessPaymentRequest
        {
            StoreId = store.Id,
            CustomerId = customer.Id,
            PaymentMethodSystemName = request.PaymentMethodSystemName
        };

        var result = await _orderProcessingService.PlaceOrderAsync(processPaymentRequest);
        if (!result.Success)
            return (null, result.Errors);

        return (result.PlacedOrder, new List<string>());
    }

    #endregion
}
