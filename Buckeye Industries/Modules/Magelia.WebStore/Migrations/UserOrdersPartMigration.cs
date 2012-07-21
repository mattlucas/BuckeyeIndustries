using System;
using System.Data;
using Magelia.WebStore.Models.Parts;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Magelia.WebStore.Migrations
{
    public class UserOrdersPartMigration : DataMigrationImpl
    {
        public Int32 Create()
        {
            this.SchemaBuilder.CreateTable(
                "UserOrdersPartRecord",
                t => t.ContentPartRecord()
                     .Column("EnableSorting", DbType.Boolean)
                     .Column("EnablePaging", DbType.Boolean)
                     .Column("PageSize", DbType.Int32)
            );
            this.ContentDefinitionManager.AlterPartDefinition(
               typeof(UserOrdersPart).Name,
               cfg => cfg.Attachable().Named("Magelia Webstore User Orders")
            );
            this.ContentDefinitionManager.AlterTypeDefinition(
                "MageliaWebstoreUserOrders",
                b => b.WithPart("UserOrdersPart")
                      .WithPart("WidgetPart")
                      .WithPart("CommonPart")
                      .WithSetting("Stereotype", "Widget")
            );
            return 1;
        }
    }
}