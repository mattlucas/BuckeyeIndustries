using System;
using Magelia.WebStore.Models.Parts;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Magelia.WebStore.Migrations
{
    public class CheckoutPartMigration : DataMigrationImpl
    {
        public Int32 Create()
        {
            this.SchemaBuilder.CreateTable(
                "CheckoutPartRecord",
                t => t
                     .ContentPartRecord()
            );
            this.ContentDefinitionManager.AlterPartDefinition(
              typeof(CheckoutPart).Name,
              cfg => cfg.Attachable().Named("Magelia Webstore Checkout")
           );
            this.ContentDefinitionManager.AlterTypeDefinition(
                "MageliaWebstoreCheckout",
                b => b.WithPart("CheckoutPart")
                      .WithPart("WidgetPart")
                      .WithPart("CommonPart")
                      .WithSetting("Stereotype", "Widget")
            );
            return 1;
        }
    }
}