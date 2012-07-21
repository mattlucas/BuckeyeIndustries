using System;
using System.Collections.Generic;
using Magelia.WebStore.Services.Contract.Data.Store;

namespace Magelia.WebStore.Models.ViewModels.User
{
    public class OrdersViewModel : List<Order>
    {
        public class OrdersState
        {
            public Nullable<OrderSortDirection> SortDirection { get; set; }
            public Nullable<OrderSortExpression> SortExpression { get; set; }
            public Nullable<Int32> Page { get; set; }
            public Int32 PageCount { get; set; }
        }

        public OrdersState State { get; set; }
    }
}