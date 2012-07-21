using System;
using Orchard.ContentManagement.Records;

namespace Magelia.WebStore.Models.Records
{
    public class ProductListPartRecord : ContentPartRecord
    {
        public virtual Boolean EnablePaging { get; set; }
        public virtual Nullable<Int32> PageSize { get; set; }
        public virtual Boolean EnableSorting { get; set; }
        public virtual Boolean FromCatalogHierarchySelection { get; set; }
        public virtual Nullable<Int32> CatalogHierarchyId { get; set; }
        public virtual String CatalogCodeFilter { get; set; }
        public virtual String CategoryCodeFilter { get; set; }
        public virtual Boolean DisplayViewDetail { get; set; }
        public virtual String ViewDetailUrlPattern { get; set; }
        public virtual Boolean AllowAddToBasket { get; set; }
    }
}