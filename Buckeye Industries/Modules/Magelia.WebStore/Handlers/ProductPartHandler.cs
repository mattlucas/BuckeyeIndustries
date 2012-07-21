using Magelia.WebStore.Models.Records;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Magelia.WebStore.Handlers
{
    public class ProductPartHandler : ContentHandler
    {
        public ProductPartHandler(IRepository<ProductPartRecord> repository)
        {
            this.Filters.Add(StorageFilter.For(repository));
        }
    }
}