using System;
using Magelia.WebStore.Contracts;
using Magelia.WebStore.Models.Parts;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;

namespace Magelia.WebStore.Drivers
{
    public class UserAddressesPartDriver : ContentPartDriver<UserAddressesPart>
    {
        private IWebStoreServices _webStoreServices;
        private IOrchardServices _orchardServices;

        protected override String Prefix
        {
            get
            {
                return "Magelia_WebStore_UserAddresses";
            }
        }

        public UserAddressesPartDriver(IWebStoreServices webStoreServices, IOrchardServices orchardServices)
        {
            this._orchardServices = orchardServices;
            this._webStoreServices = webStoreServices;
        }

        protected override DriverResult Display(UserAddressesPart part, String displayType, dynamic shapeHelper)
        {
            return this.ContentShape(
                "Parts_UserAddresses",
                () => shapeHelper.Parts_UserAddresses()
            );
        }

        protected override DriverResult Editor(UserAddressesPart part, dynamic shapeHelper)
        {
            return this.ContentShape(
                "Parts_UserAddresses_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/UserAddresses",
                    Prefix: this.Prefix,
                    Model: part
                )
            );
        }

        protected override DriverResult Editor(UserAddressesPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, this.Prefix, null, null);
            return this.Editor(part, shapeHelper);
        }
    }
}