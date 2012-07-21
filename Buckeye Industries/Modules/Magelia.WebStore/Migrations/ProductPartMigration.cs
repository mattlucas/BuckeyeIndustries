using System;
using System.Data;
using Magelia.WebStore.Models.Parts;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Magelia.WebStore.Migrations
{
    public class ProductPartMigration : DataMigrationImpl
    {
        public Int32 Create()
        {
            this.SchemaBuilder.CreateTable(
                "ProductPartRecord",
                t => t
                     .ContentPartRecord()
                     .Column("CatalogCode", DbType.String)
                     .Column("SKU", DbType.String)
                     .Column("FromUrl", DbType.Boolean)
                     .Column("CatalogCodeUrlParameterKey", DbType.String)
                     .Column("SKUUrlParameterKey", DbType.String)
                     .Column("AllowAddToBasket", DbType.Boolean)
            );
            this.ContentDefinitionManager.AlterPartDefinition(
              typeof(ProductPart).Name,
              cfg => cfg.Attachable().Named("Magelia Webstore Product")
           );
            this.ContentDefinitionManager.AlterTypeDefinition(
                "MageliaWebstoreProduct",
                b => b.WithPart("ProductPart")
                      .WithPart("WidgetPart")
                      .WithPart("CommonPart")
                      .WithSetting("Stereotype", "Widget")
            );
            return 1;
        }
    }
}