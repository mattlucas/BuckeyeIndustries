using System.Collections.Generic;
using System.Globalization;
using Magelia.WebStore.Services.Contract.Data.Store;

namespace Magelia.WebStore.Models.ViewModels.Checkout
{
    public class ShippingRatesRecapViewModel : List<ShippingRateValue>
    {
        public NumberFormatInfo NumberFormat { get; set; }
    }
}