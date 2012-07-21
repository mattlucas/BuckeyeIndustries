using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Magelia.WebStore.Models.Parts;

namespace Magelia.WebStore.Models.ViewModels.ProductList
{
    public class ProductListEditViewModel
    {
        private IEnumerable<SelectListItem> _catalogHierarchies;

        public ProductListPart Part { get; set; }
        public IEnumerable<SelectListItem> CatalogHierarchies
        {
            get
            {
                if (this._catalogHierarchies == null)
                {
                    this._catalogHierarchies = Enumerable.Empty<SelectListItem>();
                }
                return this._catalogHierarchies;
            }
            set
            {
                this._catalogHierarchies = value;
            }
        }
    }
}