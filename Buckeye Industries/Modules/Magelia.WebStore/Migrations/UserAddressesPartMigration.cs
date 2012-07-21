using System;
using Magelia.WebStore.Models.Parts;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Magelia.WebStore.Migrations
{
    public class UserAddressesPartMigration : DataMigrationImpl
    {
        public Int32 Create()
        {
            this.SchemaBuilder.CreateTable(
                "UserAddressesPartRecord",
                t => t.ContentPartRecord()
            );
            this.ContentDefinitionManager.AlterPartDefinition(
               typeof(UserAddressesPart).Name,
               cfg => cfg.Attachable().Named("Magelia Webstore User Addresses")
            );
            this.ContentDefinitionManager.AlterTypeDefinition(
                "MageliaWebstoreUserAddresses",
                b => b.WithPart("UserAddressesPart")
                      .WithPart("WidgetPart")
                      .WithPart("CommonPart")
                      .WithSetting("Stereotype", "Widget")
            );
            return 1;
        }
    }
}