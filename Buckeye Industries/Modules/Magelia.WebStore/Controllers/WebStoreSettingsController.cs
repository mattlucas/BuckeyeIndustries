using System;
using System.ServiceModel;
using System.Web.Mvc;
using Magelia.WebStore.Contracts;
using Orchard.Localization;

namespace Magelia.WebStore.Controllers
{
    public class WebStoreSettingsController : Controller
    {
        private IWebStoreServices _webStoreServices;
        private Localizer _localizer;

        public WebStoreSettingsController(IWebStoreServices webStoreServices)
        {
            this._localizer = NullLocalizer.Instance;
            this._webStoreServices = webStoreServices;
        }

        [HttpPost]
        public JsonResult Test(Guid storeId, String servicesPath)
        {
            String message = "Services successfuly tested";
            Exception exception = this._webStoreServices.UsingClient(storeId, servicesPath, c => c.StoreClient.GetContext());
            if (exception != null)
            {
                if (exception is ProtocolException)
                {
                    message = "Services couldn't be reached at the specified path";
                }
                else if (exception is UriFormatException)
                {
                    message = "Invalid service path";
                }
                else if (exception is FaultException)
                {
                    message = "Services don't respond with the specified store ID";
                }
                else
                {
                    message = "An error has occured during services connection, please check parameters and services connectivity";
                }
            }
            return this.Json(new { message = this._localizer(message).ToString() });
        }
    }
}