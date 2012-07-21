using System;
using Magelia.WebStore.Contracts;
using Magelia.WebStore.Models.Parts;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;

namespace Magelia.WebStore.Drivers
{
    public class SettingsPartDriver : ContentPartDriver<SettingsPart>
    {
        private IWebStoreServices _webStoreServices;
        private IOrchardServices _orcharServices;
        private INotifier _notifier;
        private Localizer _localizer;

        protected override String Prefix
        {
            get
            {
                return "Magelia_WebStore_Settings";
            }
        }

        private void EnsureCurrentUser()
        {
            this._webStoreServices.EnsureUser(this._orcharServices.WorkContext.CurrentUser);
        }

        protected override DriverResult Editor(SettingsPart part, dynamic shapeHelper)
        {
            return this.ContentShape(
                "Parts_Settings_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/Settings",
                    Prefix: this.Prefix,
                    Model: part
                )
            ).OnGroup("WebStoreSettings");
        }

        protected override DriverResult Editor(SettingsPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            if (updater.TryUpdateModel(part, this.Prefix, null, null))
            {
                this.EnsureCurrentUser();
            }
            return this.Editor(part, shapeHelper);
        }

        public SettingsPartDriver(INotifier notifier, IWebStoreServices webStoreServices, IOrchardServices orcharServices)
        {
            this._notifier = notifier;
            this._localizer = NullLocalizer.Instance;
            this._webStoreServices = webStoreServices;
            this._orcharServices = orcharServices;
        }
    }
}