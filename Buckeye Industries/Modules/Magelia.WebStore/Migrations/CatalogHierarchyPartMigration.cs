using System;
using System.Data;
using Magelia.WebStore.Models.Parts;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Magelia.WebStore.Migrations
{
    public class CatalogHierarchyPartMigration : DataMigrationImpl
    {
        public Int32 Create()
        {
            this.SchemaBuilder.CreateTable(
                "CatalogHierarchyPartRecord",
                t => t
                     .ContentPartRecord()
                     .Column("GenerateUrls", DbType.Boolean)
                     .Column("CatalogUrlPattern", DbType.String)
                     .Column("CategoryUrlPattern", DbType.String)
            );
            this.ContentDefinitionManager.AlterPartDefinition(
              typeof(CatalogHierarchyPart).Name,
              cfg => cfg.Attachable().Named("Magelia Webstore Catalogs and Categories")
           );
            this.ContentDefinitionManager.AlterTypeDefinition(
                "MageliaWebstoreCatalogsAndCategories",
                b => b.WithPart("CatalogHierarchyPart")
                      .WithPart("WidgetPart")
                      .WithPart("CommonPart")
                      .WithSetting("Stereotype", "Widget")
            );
            return 1;
        }
    }
}