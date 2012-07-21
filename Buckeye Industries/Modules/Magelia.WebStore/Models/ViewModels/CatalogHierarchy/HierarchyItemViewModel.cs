using System;
using System.Collections.Generic;

namespace Magelia.WebStore.Models.ViewModels.CatalogHierarchy
{
    public abstract class HierarchyItemViewModel
    {
        private List<CategoryItemViewModel> _categories;

        public Boolean Expanded { get; set; }
        public Boolean Selected { get; set; }

        public List<CategoryItemViewModel> Categories
        {
            get
            {
                if (this._categories == null)
                {
                    this._categories = new List<CategoryItemViewModel>();
                }
                return this._categories;
            }
            set
            {
                this._categories = value;
            }
        }
    }
}