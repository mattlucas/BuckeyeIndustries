using System;
using System.Globalization;
using Magelia.WebStore.Client;
using Magelia.WebStore.Services.Contract.Data.Store;
using Orchard;
using Orchard.Security;

namespace Magelia.WebStore.Contracts
{
    public interface IWebStoreServices : IDependency
    {
        Int32 CurrentCountryId { get; set; }
        Nullable<Guid> CurrentRegionId { get; set; }
        Int32 CurrentCurrencyId { get; set; }
        StoreContext StoreContext { get; }
        NumberFormatInfo NumberFormat { get; }
        String AnonymousUserName { get; }
        String CurrentUserName { get; }
        void EnsureUser(IUser user);
        Exception UsingClient(Guid storeId, String servicesPath, Action<WebStoreClient> action);
        Exception UsingClient(Action<WebStoreClient> action);
    }
}
