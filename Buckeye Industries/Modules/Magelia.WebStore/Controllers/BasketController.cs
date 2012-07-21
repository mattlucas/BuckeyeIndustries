using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Magelia.WebStore.Contracts;
using Magelia.WebStore.Extensions;
using Magelia.WebStore.Models.Parts;
using Magelia.WebStore.Models.ViewModels.Basket;
using Magelia.WebStore.Services.Contract.Data.Store;
using Magelia.WebStore.Services.Contract.Parameters.Store;
using Orchard;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Mvc;

namespace Magelia.WebStore.Controllers
{
    public class BasketController : Controller
    {
        private IWebStoreServices _webStoreService;
        private IOrchardServices _orchardServices;
        private dynamic _shapeFactory;
        private Localizer _localizer;

        public BasketController(IWebStoreServices webStoreServices, IOrchardServices orchardServices, IShapeFactory shapeFactory)
        {
            this._localizer = NullLocalizer.Instance;
            this._webStoreService = webStoreServices;
            this._orchardServices = orchardServices;
            this._shapeFactory = shapeFactory;
        }

        [HttpPost]
        public JsonResult AddToBasket(Guid productId, Int32 quantity)
        {
            Boolean success = false;
            Int32 addedQuantity = 0;
            Int32 totalQuantity = 0;
            if (quantity > 0)
            {
                Exception exception = this._webStoreService.UsingClient(
                    c =>
                    {
                        Int32 initialQuantity = c.StoreClient.GetBaskets(new[] { "default" }).SelectMany(b => b.Packages).SelectMany(p => p.LineItems).Where(li => li.ProductId == productId).Sum(li => li.Quantity);
                        BasketEntryResult result = c.StoreClient.AddProductsToBasket("default", new Dictionary<Guid, Int32> { { productId, quantity } }, this._webStoreService.CurrentCurrencyId, new Location { CountryId = this._webStoreService.CurrentCountryId, RegionId = this._webStoreService.CurrentRegionId }).FirstOrDefault(ber => ber.ProductId == productId);
                        if (result == null)
                        {
                            success = false;
                        }
                        else if (result.State != BasketEntryState.PartialyDispatched && result.State != BasketEntryState.Dispatched)
                        {
                            success = true;
                        }
                        else
                        {
                            success = true;
                            totalQuantity = result.Quantity;
                            addedQuantity = totalQuantity - initialQuantity;
                        }
                    }
                );
                if (exception != null)
                {
                    success = false;
                }
            }
            return this.Json(new { success = success, totalQuantity = totalQuantity, addedQuantity = addedQuantity });
        }

        [HttpGet]
        public ActionResult BasketCount(Int32 basketCountPartId)
        {
            BasketCountViewModel viewModel = new BasketCountViewModel { BasketUrl = this._orchardServices.ContentManager.Get<BasketCountPart>(basketCountPartId, VersionOptions.Published).BasketUrl };
            this._webStoreService.UsingClient(c => viewModel.Count = c.StoreClient.GetBasketsProductsCount(new[] { "default" }).Select(r => r.Value).FirstOrDefault());
            this.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            return new ShapePartialResult(this, this._shapeFactory.DisplayTemplate(TemplateName: "Basket/BasketCount", Model: viewModel));
        }

        [HttpGet]
        public ActionResult GetBasket(Int32 basketPartId, String currentPromoCode, String message)
        {
            BasketPart basketPart = this._orchardServices.ContentManager.Get<BasketPart>(basketPartId, VersionOptions.Published);
            BasketViewModel viewModel = new BasketViewModel
            {
                ReadOnly = basketPart.ReadOnly,
                CheckoutUrl = basketPart.CheckoutUrl,
                NumberFormat = this._webStoreService.NumberFormat,
                CurrentPromoCode = currentPromoCode,
                Message = message
            };
            this._webStoreService.UsingClient(c => viewModel.Basket = c.StoreClient.GetBasket("default"));
            this.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            this.ModelState.Clear();
            return new ShapePartialResult(this, this._shapeFactory.EditorTemplate(TemplateName: "Basket/Basket", Model: viewModel));
        }

        [HttpPost]
        public ActionResult UpdateProductQuantity(Int32 basketPartId, Guid packageId, Guid productId, Int32 quantity)
        {
            String message = null;
            if (quantity >= 0)
            {
                if (this._webStoreService.UsingClient(c => c.StoreClient.UpdateProductsQuantities("default", new Dictionary<Guid, Int32> { { productId, quantity + c.StoreClient.GetBaskets(new[] { "defaukt" }).SelectMany(b => b.Packages).Where(p => p.PackageId != packageId).SelectMany(p => p.LineItems).Where(li => li.ProductId == productId).Sum(li => li.Quantity) } })) != null)
                {
                    message = this._localizer("An unexpected error has occured").ToString();
                }
            }
            return this.GetBasket(basketPartId, null, message);
        }

        [HttpPost]
        public ActionResult AddPromoCode(Int32 basketPartId, String promoCode)
        {
            String message = null;
            String currentPromoCode = null;
            if (!String.IsNullOrEmpty(promoCode))
            {
                Exception exception = this._webStoreService.UsingClient(
                    c =>
                    {
                        if (!c.StoreClient.ApplyPromoCodes("default", promoCode).Any(pc => pc.PromoCode.EqualsInvariantCultureIgnoreCase(promoCode) && pc.State == PromoCodeEntryState.Added))
                        {
                            currentPromoCode = promoCode;
                            message = this._localizer("Promo code is not applicable").ToString();
                        }
                    }
                );
                if (exception != null)
                {
                    currentPromoCode = promoCode;
                    message = this._localizer("An unexpected error has occured").ToString();
                }
            }
            return this.GetBasket(basketPartId, currentPromoCode, message);
        }

        [HttpPost]
        public ActionResult RemovePromoCode(Int32 basketPartId, String promoCode)
        {
            String message = null;
            if (!String.IsNullOrEmpty(promoCode))
            {
                if (this._webStoreService.UsingClient(c => c.StoreClient.RemovePromoCodes("default", promoCode)) != null)
                {
                    message = this._localizer("An unexpected error has occured").ToString();
                }
            }
            return this.GetBasket(basketPartId, null, message);
        }

        [HttpGet]
        public ActionResult UpdateBasket(Int32 basketPartId)
        {
            String message = null;
            Exception exception = this._webStoreService.UsingClient(
                c =>
                {
                    if (c.StoreClient.UpdateBasket("basket", this._webStoreService.CurrentUserName, this._webStoreService.CurrentCurrencyId, CultureInfo.GetCultureInfo(this._orchardServices.WorkContext.CurrentCulture).LCID, new Location { CountryId = this._webStoreService.CurrentCountryId, RegionId = this._webStoreService.CurrentRegionId }).Any(ber => ber.State != BasketEntryState.Dispatched))
                    {
                        message = this._localizer("Some products are no more available").ToString();
                    }
                }
            );
            if (exception != null)
            {
                message = this._localizer("An unexpected error has occured").ToString();
            }
            return this.GetBasket(basketPartId, null, message);
        }

        [HttpGet]
        public ActionResult ClearBasket(Int32 basketPartId)
        {
            String message = null;
            if (this._webStoreService.UsingClient(c => c.StoreClient.DeleteBaskets(new[] { "default" })) != null)
            {
                message = this._localizer("An unexpected error has occured").ToString();
            }
            return this.GetBasket(basketPartId, null, message);
        }
    }
}