using System;
using System.Collections.Generic;
using Magelia.WebStore.Contracts;
using Magelia.WebStore.Models.Parts;
using Magelia.WebStore.Services.Contract.Data.Store;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;

namespace Magelia.WebStore.Drivers
{
    public class LocationPickerPartDriver : ContentPartDriver<LocationPickerPart>
    {
        private IWebStoreServices _webStoreServices;
        private IOrchardServices _orchardServices;

        protected override String Prefix
        {
            get
            {
                return "Magelia_WebStore_LocationPicker";
            }
        }

        private IEnumerable<Region> GetRegions()
        {
            IEnumerable<Region> regions = null;
            this._webStoreServices.UsingClient(c => regions = c.StoreClient.GetRegions(this._webStoreServices.CurrentCountryId));
            return regions;
        }

        protected override DriverResult Display(LocationPickerPart part, String displayType, dynamic shapeHelper)
        {
            SettingsPart webStoreSettings = this._orchardServices.WorkContext.CurrentSite.As<SettingsPart>();
            return this.ContentShape(
                "Parts_LocationPicker",
                () => shapeHelper.Parts_LocationPicker(
                    LocationPickerPartId: part.Id,
                    CurrentRegionId: this._webStoreServices.CurrentRegionId,
                    Regions: webStoreSettings.AllowRegionNavigation ? this.GetRegions() : null,
                    CurrentCountryId: this._webStoreServices.CurrentCountryId,
                    Countries: this._webStoreServices.StoreContext.AvailableCountries
                )
            );
        }

        protected override DriverResult Editor(LocationPickerPart part, dynamic shapeHelper)
        {
            return this.ContentShape(
                "Parts_LocationPicker_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/LocationPicker",
                    Prefix: this.Prefix,
                    Model: part
                )
            );
        }

        protected override DriverResult Editor(LocationPickerPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, this.Prefix, null, null);
            return this.Editor(part, shapeHelper);
        }

        public LocationPickerPartDriver(IOrchardServices orchardServices, IWebStoreServices webStoreServices)
        {
            this._orchardServices = orchardServices;
            this._webStoreServices = webStoreServices;
        }
    }
}