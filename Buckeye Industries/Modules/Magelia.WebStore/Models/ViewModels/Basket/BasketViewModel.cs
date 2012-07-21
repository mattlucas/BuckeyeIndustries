using System;
using System.Globalization;
using Contract = Magelia.WebStore.Services.Contract.Data.Store;

namespace Magelia.WebStore.Models.ViewModels.Basket
{
    public class BasketViewModel
    {
        public Contract.Basket Basket { get; set; }
        public Boolean ReadOnly { get; set; }
        public NumberFormatInfo NumberFormat { get; set; }
        public String CheckoutUrl { get; set; }
        public String CurrentPromoCode { get; set; }
        public String Message { get; set; }
    }
}