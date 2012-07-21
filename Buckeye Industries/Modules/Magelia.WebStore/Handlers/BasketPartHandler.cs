using Magelia.WebStore.Models.Records;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Magelia.WebStore.Handlers
{
    public class BasketPartHandler : ContentHandler
    {
        public BasketPartHandler(IRepository<BasketPartRecord> repository)
        {
            this.Filters.Add(StorageFilter.For(repository));
        }
    }
}