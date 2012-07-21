using Magelia.WebStore.Models.Records;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Magelia.WebStore.Handlers
{
    public class BasketCountPartHandler : ContentHandler
    {
        public BasketCountPartHandler(IRepository<BasketCountPartRecord> repository)
        {
            this.Filters.Add(StorageFilter.For(repository));
        }
    }
}