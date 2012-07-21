using System;
using Orchard.ContentManagement.Records;

namespace Magelia.WebStore.Models.Records
{
    public class SettingsPartRecord : ContentPartRecord
    {
        public virtual Guid StoreId { get; set; }
        public virtual String ServicesPath { get; set; }
        public virtual Boolean AllowRegionNavigation { get; set; }
    }
}