using System;
using Orchard.ContentManagement.Records;

namespace Magelia.WebStore.Models.Records
{
    public class BasketCountPartRecord : ContentPartRecord
    {
        public virtual String BasketUrl { get; set; }
    }
}