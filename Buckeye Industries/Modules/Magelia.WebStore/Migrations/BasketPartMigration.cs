using System;
using System.Data;
using Magelia.WebStore.Models.Parts;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Magelia.WebStore.Migrations
{
    public class BasketPartMigration : DataMigrationImpl
    {
        public Int32 Create()
        {
            this.SchemaBuilder.CreateTable(
               "BasketPartRecord",
               t => t
                    .ContentPartRecord()
                    .Column("ReadOnly", DbType.Boolean)
                    .Column("CheckoutUrl", DbType.String)
           );
            this.ContentDefinitionManager.AlterPartDefinition(
              typeof(BasketPart).Name,
              cfg => cfg.Attachable().Named("Magelia Webstore Basket")
           );
            this.ContentDefinitionManager.AlterTypeDefinition(
                "MageliaWebstoreBasket",
                b => b.WithPart("BasketPart")
                      .WithPart("WidgetPart")
                      .WithPart("CommonPart")
                      .WithSetting("Stereotype", "Widget")
            );
            return 1;
        }
    }
}