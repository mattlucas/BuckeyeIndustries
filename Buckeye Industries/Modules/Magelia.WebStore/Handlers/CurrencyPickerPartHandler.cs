using Magelia.WebStore.Models.Records;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Magelia.WebStore.Handlers
{
    public class CurrencyPickerPartHandler : ContentHandler
    {
        public CurrencyPickerPartHandler(IRepository<CurrencyPickerPartRecord> repository)
        {
            this.Filters.Add(StorageFilter.For(repository));
        }
    }
}