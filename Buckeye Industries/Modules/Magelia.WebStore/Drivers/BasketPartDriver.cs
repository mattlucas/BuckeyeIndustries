using System;
using Magelia.WebStore.Models.Parts;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;

namespace Magelia.WebStore.Drivers
{
    public class BasketPartDriver : ContentPartDriver<BasketPart>
    {
        protected override String Prefix
        {
            get
            {
                return "Magelia_WebStore_Basket";
            }
        }

        protected override DriverResult Display(BasketPart part, String displayType, dynamic shapeHelper)
        {
            return this.ContentShape(
                "Parts_Basket",
                () => shapeHelper.Parts_Basket(BasketPartId: part.Id)
            );
        }

        protected override DriverResult Editor(BasketPart part, dynamic shapeHelper)
        {
            return this.ContentShape(
                "Parts_Basket_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/Basket",
                    Prefix: this.Prefix,
                    Model: part
                )
            );
        }

        protected override DriverResult Editor(BasketPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, this.Prefix, null, null);
            return this.Editor(part, shapeHelper);
        }
    }
}