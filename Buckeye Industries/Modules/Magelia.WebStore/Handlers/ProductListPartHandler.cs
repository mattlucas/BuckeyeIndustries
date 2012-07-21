using Magelia.WebStore.Models.Records;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Magelia.WebStore.Handlers
{
    public class ProductListPartHandler : ContentHandler
    {
        public ProductListPartHandler(IRepository<ProductListPartRecord> repository)
        {
            this.Filters.Add(StorageFilter.For(repository));
        }
    }
}