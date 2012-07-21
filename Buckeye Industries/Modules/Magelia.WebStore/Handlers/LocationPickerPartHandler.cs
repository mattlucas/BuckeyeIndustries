using Magelia.WebStore.Models.Records;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Magelia.WebStore.Handlers
{
    public class LocationPickerPartHandler : ContentHandler
    {
        public LocationPickerPartHandler(IRepository<LocationPickerPartRecord> repository)
        {
            this.Filters.Add(StorageFilter.For(repository));
        }
    }
}