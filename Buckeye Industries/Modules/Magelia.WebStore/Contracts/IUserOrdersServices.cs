using System;
using Magelia.WebStore.Models.Parts;
using Magelia.WebStore.Models.ViewModels.User;
using Magelia.WebStore.Services.Contract.Data.Store;
using Orchard;

namespace Magelia.WebStore.Contracts
{
    public interface IUserOrdersServices : IDependency
    {
        OrdersViewModel GetModel(UserOrdersPart part);
        void UpdateState(UserOrdersPart part, Nullable<OrderSortDirection> sortDirection, Nullable<OrderSortExpression> sortExpression, Nullable<Int32> page);
    }
}