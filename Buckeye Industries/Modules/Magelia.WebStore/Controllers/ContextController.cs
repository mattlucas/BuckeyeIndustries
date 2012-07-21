using System;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Magelia.WebStore.Contracts;
using Magelia.WebStore.Extensions;
using Magelia.WebStore.Services.Contract.Parameters.Store;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Mvc;

namespace Magelia.WebStore.Controllers
{
    public class ContextController : Controller
    {
        private IWebStoreServices _webStoreServices;
        private IOrchardServices _orchardServices;

        private RedirectResult RedirectOnReferer()
        {
            return this.Redirect(this.Request.UrlReferrer.RemoveAddedParameters().ToString());
        }

        private void UpdateBasket()
        {
            this._webStoreServices.UsingClient(c => c.StoreClient.UpdateBasket("default", this._webStoreServices.CurrentUserName, this._webStoreServices.CurrentCurrencyId, CultureInfo.GetCultureInfo(this._orchardServices.WorkContext.CurrentCulture).LCID, new Location { CountryId = this._webStoreServices.CurrentCountryId, RegionId = this._webStoreServices.CurrentRegionId }));
        }

        public ContextController(IWebStoreServices webStoreServices, IOrchardServices orchardServices)
        {
            this._webStoreServices = webStoreServices;
            this._orchardServices = orchardServices;
        }

        [HttpGet]
        public JsonResult GetRegions(Int32 countryId)
        {
            Object result = null;
            this._webStoreServices.UsingClient(c => result = c.StoreClient.GetRegions(countryId).Select(r => new { regionId = r.RegionId, name = r.Name }));
            this.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            return this.Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetCurrentLocation(Int32 locationPickerPartId)
        {
            this.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            return new ShapePartialResult(this, this._orchardServices.ContentManager.BuildDisplay(this._orchardServices.ContentManager.Get(locationPickerPartId, VersionOptions.Published)));
        }

        [HttpPost]
        public RedirectResult UpdateLocation(Int32 countryId, Nullable<Guid> regionId)
        {
            this._webStoreServices.CurrentCountryId = countryId;
            this._webStoreServices.CurrentRegionId = regionId;
            this.UpdateBasket();
            return this.RedirectOnReferer();
        }

        [HttpPost]
        public RedirectResult UpdateCurrency(Int32 currencyId)
        {
            this._webStoreServices.CurrentCurrencyId = currencyId;
            this.UpdateBasket();
            return this.RedirectOnReferer();
        }

        [HttpGet]
        public ActionResult Magelia()
        {
            this.Response.ContentType = "text/javascript";
            return this.View();
        }
    }
}