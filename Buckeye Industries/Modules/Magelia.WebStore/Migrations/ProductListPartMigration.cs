using System;
using System.Data;
using Magelia.WebStore.Models.Parts;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Magelia.WebStore.Migrations
{
    public class ProductListPartMigration : DataMigrationImpl
    {
        public Int32 Create()
        {
            this.SchemaBuilder.CreateTable(
                "ProductListPartRecord",
                t => t
                     .ContentPartRecord()
                     .Column("EnablePaging", DbType.Boolean)
                     .Column("PageSize", DbType.Int32)
                     .Column("EnableSorting", DbType.Boolean)
                     .Column("FromCatalogHierarchySelection", DbType.Boolean)
                     .Column("CatalogHierarchyId", DbType.Int32)
                     .Column("CatalogCodeFilter", DbType.String)
                     .Column("CategoryCodeFilter", DbType.String)
                     .Column("DisplayViewDetail", DbType.Boolean)
                     .Column("AllowAddToBasket", DbType.Boolean)
                     .Column("ViewDetailUrlPattern", DbType.String)
            );
            this.ContentDefinitionManager.AlterPartDefinition(
              typeof(ProductListPart).Name,
              cfg => cfg.Attachable().Named("Magelia Webstore Product List")
           );
            this.ContentDefinitionManager.AlterTypeDefinition(
                "MageliaWebstoreProductList",
                b => b.WithPart("ProductListPart")
                      .WithPart("WidgetPart")
                      .WithPart("CommonPart")
                      .WithSetting("Stereotype", "Widget")
            );
            return 1;
        }
    }
}