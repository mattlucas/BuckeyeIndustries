using System;
using Orchard.ContentManagement.Records;

namespace Magelia.WebStore.Models.Records
{
    public class ProductPartRecord : ContentPartRecord
    {
        public virtual String CatalogCode { get; set; }
        public virtual String SKU { get; set; }
        public virtual Boolean FromUrl { get; set; }
        public virtual String CatalogCodeUrlParameterKey { get; set; }
        public virtual String SKUUrlParameterKey { get; set; }
        public virtual Boolean AllowAddToBasket { get; set; }
    }
}