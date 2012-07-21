using System;
using System.Globalization;
using Magelia.WebStore.Services.Contract.Data.Store;

namespace Magelia.WebStore.Models.ViewModels.User
{
    public class OrderViewModel
    {
        public Order Order { get; set; }
        public OrderAddressViewModel BillingAddress { get; set; }
        public OrderAddressViewModel ShippingAddress { get; set; }
        public NumberFormatInfo NumberFormat { get; set; }
        public Int32 UserOrdersPartId { get; set; }
    }
}