using System;
using Magelia.WebStore.Client;

namespace Magelia.WebStore.Models.ViewModels.Product
{
    public class ProductViewModel
    {
        private BaseProduct _baseProduct;

        public String RequestedSKU { get; set; }
        public String RequestedCatalogCode { get; set; }
        public ReferenceProduct ReferenceProduct { get; set; }

        public BaseProduct BaseProduct
        {
            get
            {
                if (this._baseProduct == null && this.ReferenceProduct != null)
                {
                    this._baseProduct = this.ReferenceProduct is VariantProduct ? (this.ReferenceProduct as VariantProduct).VariableProduct as BaseProduct : this.ReferenceProduct;
                }
                return this._baseProduct;
            }
        }
    }
}