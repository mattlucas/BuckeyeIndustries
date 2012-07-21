using System;
using Orchard.ContentManagement.Records;

namespace Magelia.WebStore.Models.Records
{
    public class CatalogHierarchyPartRecord : ContentPartRecord
    {
        public virtual Boolean GenerateUrls { get; set; }
        public virtual String CatalogUrlPattern { get; set; }
        public virtual String CategoryUrlPattern { get; set; }
    }
}