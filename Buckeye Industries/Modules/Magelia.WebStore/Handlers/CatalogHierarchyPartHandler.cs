using Magelia.WebStore.Models.Records;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Magelia.WebStore.Handlers
{
    public class CatalogHierarchyPartHandler : ContentHandler
    {
        public CatalogHierarchyPartHandler(IRepository<CatalogHierarchyPartRecord> repository)
        {
            this.Filters.Add(StorageFilter.For(repository));
        }
    }
}