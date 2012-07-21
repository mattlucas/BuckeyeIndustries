using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using Magelia.WebStore.Client;

namespace Magelia.WebStore.Models.ViewModels.ProductList
{
    public class ProductListViewModel : List<BaseProduct>
    {
        public class ProductListState
        {
            public String CatalogCodeFilter { get; set; }
            public String CategoryCodeFilter { get; set; }
            public Nullable<SortDirection> SortDirection { get; set; }
            public String SortExpression { get; set; }
            public Nullable<Int32> Page { get; set; }
            public Int32 PageCount { get; set; }
            public String FromPath { get; set; }
        }

        public ProductListState State { get; set; }
    }
}