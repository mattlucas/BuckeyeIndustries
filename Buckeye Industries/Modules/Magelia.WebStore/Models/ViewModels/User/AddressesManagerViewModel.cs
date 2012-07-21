using System;

namespace Magelia.WebStore.Models.ViewModels.User
{
    public class AddressesManagerViewModel
    {
        public Boolean CanSelect { get; set; }
        public Nullable<Guid> SelectedAddressId { get; set; }
        public Boolean PromptShippingAddressIsDifferent { get; set; }
        public Boolean ShippingAddressIsDifferent { get; set; }
        public Nullable<Guid> ExceptedAddressId { get; set; }
    }
}