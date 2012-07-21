using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Magelia.WebStore.Services.Contract.Data.Store;

namespace Magelia.WebStore.Models.ViewModels.Checkout
{
    public class ShippingRatesViewModel
    {
        public class ShippingRateValueSelection
        {
            [Required]
            public Nullable<Guid> ShippingRateValueId { get; set; }
            public Guid PackageId { get; set; }
        }

        private Dictionary<Guid, IEnumerable<ShippingRateValue>> _shippingRatesByPackage;
        private List<ShippingRateValueSelection> _shippingRateValueSelections;

        public NumberFormatInfo NumberFormat { get; set; }

        public Dictionary<Guid, IEnumerable<ShippingRateValue>> ShippingRatesByPackage
        {
            get
            {
                if (this._shippingRatesByPackage == null)
                {
                    this._shippingRatesByPackage = new Dictionary<Guid, IEnumerable<ShippingRateValue>>();
                }
                return this._shippingRatesByPackage;
            }
            set
            {
                this._shippingRatesByPackage = value;
            }
        }

        public List<ShippingRateValueSelection> ShippingRateValueSelections
        {
            get
            {
                if (this._shippingRateValueSelections == null)
                {
                    this._shippingRateValueSelections = new List<ShippingRateValueSelection>();
                }
                return this._shippingRateValueSelections;
            }
            set
            {
                this._shippingRateValueSelections = value;
            }
        }
    }
}