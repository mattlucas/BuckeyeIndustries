using System;
using Magelia.WebStore.Models.Parts;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Magelia.WebStore.Migrations
{
    public class LocationPickerPartMigration : DataMigrationImpl
    {
        public Int32 Create()
        {
            this.SchemaBuilder.CreateTable(
                "LocationPickerPartRecord",
                t => t.ContentPartRecord()
            );
            this.ContentDefinitionManager.AlterPartDefinition(
              typeof(LocationPickerPart).Name,
              cfg => cfg.Attachable().Named("Magelia Webstore Country and Region Picker")
           );
            this.ContentDefinitionManager.AlterTypeDefinition(
                "MageliaWebstoreCountryAndRegionPicker",
                b => b.WithPart("LocationPickerPart")
                      .WithPart("WidgetPart")
                      .WithPart("CommonPart")
                      .WithSetting("Stereotype", "Widget")
            );
            return 1;
        }
    }
}