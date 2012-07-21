using Magelia.WebStore.Models.Records;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Magelia.WebStore.Handlers
{
    public class UserOrdersPartHandler : ContentHandler
    {
        public UserOrdersPartHandler(IRepository<UserOrdersPartRecord> repository)
        {
            this.Filters.Add(StorageFilter.For(repository));
        }
    }
}