using System;
using Magelia.WebStore.Models.Parts;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
namespace Magelia.WebStore.Migrations
{
    public class CurrencyPickerPartMigration : DataMigrationImpl
    {
        public Int32 Create()
        {
            this.SchemaBuilder.CreateTable(
                "CurrencyPickerPartRecord",
                t => t.ContentPartRecord()
            );
            this.ContentDefinitionManager.AlterPartDefinition(
              typeof(CurrencyPickerPart).Name,
              cfg => cfg.Attachable().Named("Magelia Webstore Currency Picker")
           );
            this.ContentDefinitionManager.AlterTypeDefinition(
                "MageliaWebstoreCurrencyPicker",
                b => b.WithPart("CurrencyPickerPart")
                      .WithPart("WidgetPart")
                      .WithPart("CommonPart")
                      .WithSetting("Stereotype", "Widget")
            );
            return 1;
        }
    }
}