using System;
using System.Data;
using Orchard.Data.Migration;

namespace Magelia.WebStore.Migrations
{
    public class SettingsPartMigration : DataMigrationImpl
    {
        public Int32 Create()
        {
            this.SchemaBuilder.CreateTable(
                "SettingsPartRecord",
                t => t
                    .ContentPartRecord()
                    .Column("StoreId", DbType.Guid, c => c.NotNull())
                    .Column("ServicesPath", DbType.String, c => c.Unlimited())
                    .Column("AllowRegionNavigation", DbType.Boolean)
            );
            return 1;
        }
    }
}