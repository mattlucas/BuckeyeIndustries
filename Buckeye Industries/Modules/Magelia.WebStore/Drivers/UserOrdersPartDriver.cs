using System;
using Magelia.WebStore.Contracts;
using Magelia.WebStore.Models.Parts;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;

namespace Magelia.WebStore.Drivers
{
    public class UserOrdersPartDriver : ContentPartDriver<UserOrdersPart>
    {
        private IUserOrdersServices _userOrdersServices;
        private IWebStoreServices _webStoreServices;
        private IOrchardServices _orchardServices;
        private Localizer _localizer;

        protected override String Prefix
        {
            get
            {
                return "Magelia_WebStore_UserOrders";
            }
        }

        protected override DriverResult Display(UserOrdersPart part, String displayType, dynamic shapeHelper)
        {
            return this.ContentShape(
                "Parts_UserOrders",
                () => shapeHelper.Parts_UserOrders(
                    EnablePaging: part.EnablePaging,
                    EnableSorting: part.EnableSorting,
                    PageSize: part.PageSize,
                    UserOrdersPartId: part.Id,
                    AvailableCurrencies: this._webStoreServices.StoreContext.AvailableCurrencies,
                    Authenticated: this._orchardServices.WorkContext.CurrentUser != null,
                    Orders: this._userOrdersServices.GetModel(part)
                )
            );
        }

        protected override DriverResult Editor(UserOrdersPart part, dynamic shapeHelper)
        {
            return this.ContentShape(
                "Parts_UserOrders_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/UserOrders",
                    Prefix: this.Prefix,
                    Model: part
                )
            );
        }

        protected override DriverResult Editor(UserOrdersPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, this.Prefix, null, null);
            if (part.EnablePaging && !part.PageSize.HasValue)
            {
                updater.AddModelError("PageSizeRequired", this._localizer("Page size is required"));
            }
            return this.Editor(part, shapeHelper);
        }

        public UserOrdersPartDriver(IWebStoreServices webStoreServices, IUserOrdersServices userOrdersServices, IOrchardServices orchardServices)
        {
            this._webStoreServices = webStoreServices;
            this._userOrdersServices = userOrdersServices;
            this._orchardServices = orchardServices;
            this._localizer = NullLocalizer.Instance;
        }
    }
}