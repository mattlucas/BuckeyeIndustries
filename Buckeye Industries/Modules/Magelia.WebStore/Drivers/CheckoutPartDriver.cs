using System;
using System.Linq;
using Magelia.WebStore.Contracts;
using Magelia.WebStore.Models.Parts;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;

namespace Magelia.WebStore.Drivers
{
    public class CheckoutPartDriver : ContentPartDriver<CheckoutPart>
    {
        private IWebStoreServices _webStoreServices;

        protected override String Prefix
        {
            get
            {
                return "Magelia_WebStore_Checkout";
            }
        }

        protected override DriverResult Display(CheckoutPart part, String displayType, dynamic shapeHelper)
        {
            Boolean empty = true;
            this._webStoreServices.UsingClient(c => empty = !c.StoreClient.GetBasket("default").Packages.SelectMany(p => p.LineItems).Any());
            return this.ContentShape(
                "Parts_Checkout",
                () => shapeHelper.Parts_Checkout(Empty: empty)
            );
        }

        protected override DriverResult Editor(CheckoutPart part, dynamic shapeHelper)
        {
            return this.ContentShape(
                "Parts_Checkout_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/Checkout",
                    Prefix: this.Prefix,
                    Model: part
                )
            );
        }

        protected override DriverResult Editor(CheckoutPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, this.Prefix, null, null);
            return this.Editor(part, shapeHelper);
        }

        public CheckoutPartDriver(IWebStoreServices webStoreServices)
        {
            this._webStoreServices = webStoreServices;
        }
    }
}