using System;
using System.Data;
using Magelia.WebStore.Models.Parts;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Magelia.WebStore.Migrations
{
    public class BasketCountPartMigration : DataMigrationImpl
    {
        public Int32 Create()
        {
            this.SchemaBuilder.CreateTable(
               "BasketCountPartRecord",
               t => t
                    .ContentPartRecord()
                    .Column("BasketUrl", DbType.String)
           );
            this.ContentDefinitionManager.AlterPartDefinition(
              typeof(BasketCountPart).Name,
              cfg => cfg.Attachable().Named("Magelia Webstore Basket Count")
           );
            this.ContentDefinitionManager.AlterTypeDefinition(
                "MageliaWebstoreBasketCount",
                b => b.WithPart("BasketCountPart")
                      .WithPart("WidgetPart")
                      .WithPart("CommonPart")
                      .WithSetting("Stereotype", "Widget")
            );
            return 1;
        }
    }
}