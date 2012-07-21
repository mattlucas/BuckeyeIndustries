using Magelia.WebStore.Models.Records;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Magelia.WebStore.Handlers
{
    public class CheckoutPartHandler : ContentHandler
    {
        public CheckoutPartHandler(IRepository<CheckoutPartRecord> repository)
        {
            this.Filters.Add(StorageFilter.For(repository));
        }
    }
}