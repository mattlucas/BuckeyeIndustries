using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Magelia.WebStore.Contracts;
using Magelia.WebStore.Extensions;
using Magelia.WebStore.Models.ViewModels.Checkout;
using Magelia.WebStore.Models.ViewModels.User;
using Magelia.WebStore.Services.Contract.Data.Store;
using Orchard;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Mvc;
using Customer = Magelia.WebStore.Services.Contract.Data.Customer;

namespace Magelia.WebStore.Controllers
{
    public class CheckoutController : Controller
    {
        private const String ShippingAddressIdSessionKey = "shippingaddressid";
        private const String BillingAddressIdSessionKey = "billingaddressid";
        private const String ShippingAddressIsDifferentSessionKey = "shippingaddressisdifferent";
        private const String RawShippingAddressSessionKey = "rawshippingaddress";
        private const String RawBillingAddressSessionKey = "raxbillingaddress";

        private IWebStoreServices _webStoreServices;
        private IOrchardServices _orchardServices;
        private dynamic _shapeFactory;
        private Localizer _localizer;

        private Nullable<Guid> ShippingAddressId
        {
            get
            {
                return this.Session[CheckoutController.ShippingAddressIdSessionKey] as Nullable<Guid>;
            }
            set
            {
                this.Session[CheckoutController.ShippingAddressIdSessionKey] = value;
            }
        }

        private Nullable<Guid> BillingAddressId
        {
            get
            {
                return this.Session[CheckoutController.BillingAddressIdSessionKey] as Nullable<Guid>;
            }
            set
            {
                this.Session[CheckoutController.BillingAddressIdSessionKey] = value;
            }
        }

        private Boolean ShippingAddressIsDifferent
        {
            get
            {
                Object rawShippingAddressIsDifferent = this.Session[CheckoutController.ShippingAddressIsDifferentSessionKey];
                return rawShippingAddressIsDifferent == null ? false : (Boolean)rawShippingAddressIsDifferent;
            }
            set
            {
                this.Session[CheckoutController.ShippingAddressIsDifferentSessionKey] = value;
            }
        }

        private Address RawShippingAddress
        {
            get
            {
                if (this.Session[CheckoutController.RawShippingAddressSessionKey] == null)
                {
                    this.RawShippingAddress = this.GetDefaultRawAddress();
                }
                return this.Session[CheckoutController.RawShippingAddressSessionKey] as Address;
            }
            set
            {
                this.Session[CheckoutController.RawShippingAddressSessionKey] = value;
            }
        }

        private Address RawBillingAddress
        {
            get
            {
                if (this.Session[CheckoutController.RawBillingAddressSessionKey] == null)
                {
                    this.RawBillingAddress = this.GetDefaultRawAddress();
                }
                return this.Session[CheckoutController.RawBillingAddressSessionKey] as Address;
            }
            set
            {
                this.Session[CheckoutController.RawBillingAddressSessionKey] = value;
            }
        }

        private Boolean _isAnonymous
        {
            get
            {
                return this._orchardServices.WorkContext.CurrentUser == null;
            }
        }

        private Address GetDefaultRawAddress()
        {
            return new Address { CountryId = this._webStoreServices.CurrentCountryId, RegionId = this._webStoreServices.CurrentRegionId };
        }

        private AddressViewModel Initialize(AddressViewModel viewModel)
        {
            viewModel.Countries = this._webStoreServices.StoreContext.AvailableCountries.Select(ac => new SelectListItem { Text = ac.Name, Value = ac.CountryId.ToString() });
            if (viewModel.CountryId.HasValue)
            {
                this._webStoreServices.UsingClient(c => viewModel.Regions = c.StoreClient.GetRegions(viewModel.CountryId.Value).Select(r => new SelectListItem { Text = r.Name, Value = r.RegionId.ToString() }));
            }
            return viewModel;
        }

        private AddressViewModel GetAddressViewModel(Address address)
        {
            Boolean isBillingAddress = address == this.RawBillingAddress;
            AddressViewModel viewModel = new AddressViewModel
            {
                AddressId = address.AddressId,
                City = address.City,
                Company = address.Company,
                CountryId = address.CountryId,
                DigiCode = address.DigiCode,
                Email = address.Email,
                FaxNumber = address.FaxNumber,
                FirstName = address.FirstName,
                Floor = address.Floor,
                LastName = address.LastName,
                Line1 = address.Line1,
                Line2 = address.Line2,
                Line3 = address.Line3,
                MiddleName = address.MiddleName,
                MobileNumber = address.MobileNumber,
                PhoneNumber = address.PhoneNumber,
                RegionId = address.RegionId,
                ZipCode = address.ZipCode,
                PromptShippingAddressIsDifferent = isBillingAddress,
                PromptEmail = isBillingAddress,
                ShippingAddressIsDifferent = this.ShippingAddressIsDifferent,
                DisplayNexButton = true,
                Named = false
            };
            this.Initialize(viewModel);
            return viewModel;
        }

        private AddressRecapViewModel GetAddressRecapViewModel(Customer.Address address)
        {
            if (address != null)
            {
                return new AddressRecapViewModel
                {
                    City = address.City,
                    Company = address.Company,
                    CountryName = address.CountryName,
                    DigiCode = address.DigiCode,
                    Email = address.Email,
                    FaxNumber = address.FaxNumber,
                    FirstName = address.FirstName,
                    Floor = address.Floor,
                    LastName = address.LastName,
                    Line1 = address.Line1,
                    Line2 = address.Line2,
                    Line3 = address.Line3,
                    MiddleName = address.MiddleName,
                    MobileNumber = address.MobileNumber,
                    Name = address.Name,
                    PhoneNumber = address.PhoneNumber,
                    RegionName = address.RegionName,
                    ZipCode = address.ZipCode,
                    ShippingAddressIsTheSame = address.AddressId == this.BillingAddressId && !this.ShippingAddressIsDifferent
                };
            }
            return null;
        }

        private AddressRecapViewModel GetAddressRecapViewModel(Address address)
        {
            if (address != null)
            {
                AddressRecapViewModel viewModel = new AddressRecapViewModel
                {
                    City = address.City,
                    Company = address.Company,
                    CountryName = this._webStoreServices.StoreContext.AvailableCountries.Where(ac => ac.CountryId == address.CountryId).Select(ac => ac.Name).FirstOrDefault(),
                    DigiCode = address.DigiCode,
                    Email = address.Email,
                    FaxNumber = address.FaxNumber,
                    FirstName = address.FirstName,
                    Floor = address.Floor,
                    LastName = address.LastName,
                    Line1 = address.Line1,
                    Line2 = address.Line2,
                    Line3 = address.Line3,
                    MiddleName = address.MiddleName,
                    MobileNumber = address.MobileNumber,
                    PhoneNumber = address.PhoneNumber,
                    ZipCode = address.ZipCode,
                    ShippingAddressIsTheSame = address == this.RawBillingAddress && !this.ShippingAddressIsDifferent
                };
                if (address.RegionId.HasValue)
                {
                    this._webStoreServices.UsingClient(c => viewModel.RegionName = c.StoreClient.GetRegions(address.CountryId).Where(r => r.RegionId == address.RegionId).Select(r => r.Name).FirstOrDefault());
                }
                return viewModel;
            }
            return null;
        }

        private ActionResult BuildAddressRecapShapeResult(AddressRecapViewModel viewModel)
        {
            return new ShapePartialResult(this, this._shapeFactory.DisplayTemplate(TemplateName: "Checkout/AddressRecap", Model: viewModel));
        }

        private ActionResult BuildCustomerAddressRecap(Nullable<Guid> addressId)
        {
            Customer.Address address = null;
            if (addressId.HasValue)
            {
                this._webStoreServices.UsingClient(c => address = c.CustomerClient.GetAddresses(this._webStoreServices.CurrentUserName).FirstOrDefault(a => a.AddressId == addressId));
            }
            return this.BuildAddressRecapShapeResult(this.GetAddressRecapViewModel(address));
        }

        private ActionResult BuildRawBillingAddressRecap()
        {
            return this.BuildAddressRecapShapeResult(this.GetAddressRecapViewModel(this.RawBillingAddress));
        }

        private ActionResult BuildRawShippingAddressRecap()
        {
            return this.BuildAddressRecapShapeResult(this.GetAddressRecapViewModel(this.RawShippingAddress));
        }

        private ActionResult BuildBillingAddressRecap()
        {
            return this.BuildCustomerAddressRecap(this.BillingAddressId);
        }

        private ActionResult BuildShippingAddressRecap()
        {
            return this.BuildCustomerAddressRecap(this.ShippingAddressId);
        }

        private ActionResult EditAddress(Boolean isBillingAddress)
        {
            dynamic addressEditor;
            if (this._isAnonymous)
            {
                addressEditor = this._shapeFactory.EditorTemplate(TemplateName: "User/Address", Model: this.GetAddressViewModel(isBillingAddress ? this.RawBillingAddress : this.RawShippingAddress));
            }
            else
            {
                addressEditor = this._shapeFactory.EditorTemplate(TemplateName: "User/AddressesManager", Model: new AddressesManagerViewModel { CanSelect = true, SelectedAddressId = isBillingAddress ? this.BillingAddressId : this.ShippingAddressId, PromptShippingAddressIsDifferent = isBillingAddress, ShippingAddressIsDifferent = this.ShippingAddressIsDifferent, ExceptedAddressId = isBillingAddress ? null : this.BillingAddressId });
            }
            return new ShapePartialResult(this, addressEditor);
        }

        private void SetAddresses(Guid billingAddress, Nullable<Guid> shippingAddressId)
        {
            this._webStoreServices.UsingClient(
                c =>
                {
                    Guid finalShippingAddressId = shippingAddressId ?? billingAddress;
                    Customer.Address address = c.CustomerClient.GetAddresses(this._webStoreServices.CurrentUserName).FirstOrDefault(a => a.AddressId == finalShippingAddressId);
                    this._webStoreServices.CurrentCountryId = address.CountryId;
                    this._webStoreServices.CurrentRegionId = address.RegionId;
                    c.StoreClient.SetCustomerAddressToBasket("default", billingAddress, shippingAddressId);
                }
            );
        }

        private void SetAddresses(Address rawBillingAddress, Address rawShippingAddress)
        {
            Address finalShippingAddress = rawShippingAddress ?? rawBillingAddress;
            this._webStoreServices.CurrentCountryId = finalShippingAddress.CountryId;
            this._webStoreServices.CurrentRegionId = finalShippingAddress.RegionId;
            this._webStoreServices.UsingClient(c => c.StoreClient.SetRawAddressToBasket("default", rawBillingAddress, rawShippingAddress));
        }

        private void UpdateAddress(Address address, AddressViewModel viewModel)
        {
            address.City = viewModel.City;
            address.Company = viewModel.Company;
            address.CountryId = viewModel.CountryId.Value;
            address.DigiCode = viewModel.DigiCode;
            address.Email = viewModel.Email;
            address.FaxNumber = viewModel.FaxNumber;
            address.FirstName = viewModel.FirstName;
            address.Floor = viewModel.Floor;
            address.LastName = viewModel.LastName;
            address.Line1 = viewModel.Line1;
            address.Line2 = viewModel.Line2;
            address.Line3 = viewModel.Line3;
            address.MiddleName = viewModel.MiddleName;
            address.MobileNumber = viewModel.MobileNumber;
            address.PhoneNumber = viewModel.PhoneNumber;
            address.RegionId = viewModel.RegionId;
            address.ZipCode = viewModel.ZipCode;
        }

        private ActionResult RegisterAddress(Address address, AddressViewModel viewModel)
        {
            if (this.ModelState.IsValid)
            {
                Boolean success = false;
                this._webStoreServices.UsingClient(
                    c =>
                    {
                        if (viewModel.RegionId.HasValue || (!viewModel.RegionId.HasValue && !c.StoreClient.GetRegions(viewModel.CountryId.Value).Any()))
                        {
                            success = true;
                        }
                        else
                        {
                            success = false;
                            this.ModelState.AddModelError("RegionRequired", this._localizer("Region is required").ToString());
                        }
                    }
                );
                if (success)
                {
                    this.UpdateAddress(address, viewModel);
                    if (address == this.RawBillingAddress)
                    {
                        this.ShippingAddressIsDifferent = viewModel.ShippingAddressIsDifferent;
                        if (!this.ShippingAddressIsDifferent)
                        {
                            this.SetAddresses(this.RawBillingAddress, null);
                        }
                        return this.Json(new { shippingAddressIsDifferent = this.ShippingAddressIsDifferent });
                    }
                    else
                    {
                        this.SetAddresses(this.RawBillingAddress, this.RawShippingAddress);
                        return this.Json(new { success = true });
                    }
                }
            }
            this.Initialize(viewModel);
            return new ShapePartialResult(this, this._shapeFactory.EditorTemplate(TemplateName: "User/Address", Model: viewModel));
        }

        private void Initialize(ShippingRatesViewModel viewModel)
        {
            viewModel.NumberFormat = this._webStoreServices.NumberFormat;
            this._webStoreServices.UsingClient(c => viewModel.ShippingRatesByPackage = c.StoreClient.GetShippingRateValues("default"));
        }

        private ActionResult BuildShippingRatesShapeResult(ShippingRatesViewModel viewModel)
        {
            return new ShapePartialResult(this, this._shapeFactory.EditorTemplate(TemplateName: "Checkout/ShippingRates", Model: viewModel));
        }

        private ActionResult BuildPaymentShape(PaymentViewModel viewModel)
        {
            return new ShapePartialResult(this, this._shapeFactory.EditorTemplate(TemplateName: "Checkout/Payment", Model: viewModel));
        }

        private ActionResult BuildProceedToPaymentShape(ProceedToPaymentViewModel viewModel)
        {
            return new ShapePartialResult(this, this._shapeFactory.DisplayTemplate(TemplateName: "Checkout/ProceedToPayment", Model: viewModel));
        }

        public CheckoutController(IWebStoreServices webStoreServices, IOrchardServices orchardServices, IShapeFactory shapeFactory)
        {
            this._localizer = NullLocalizer.Instance;
            this._webStoreServices = webStoreServices;
            this._orchardServices = orchardServices;
            this._shapeFactory = shapeFactory;
        }

        [HttpGet]
        public ActionResult EditBillingAddress()
        {
            this.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            return this.EditAddress(true);
        }

        [HttpGet]
        public ActionResult EditShippingAddress()
        {
            this.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            return this.EditAddress(false);
        }

        [HttpPost]
        [Authorize]
        public ActionResult SelectBillingAddress(Guid billingAddressId, Boolean shippingAddressIsDifferent)
        {
            this.BillingAddressId = billingAddressId;
            this.ShippingAddressIsDifferent = shippingAddressIsDifferent;
            if (!this.ShippingAddressIsDifferent)
            {
                this.SetAddresses(billingAddressId, null);
            }
            return this.BuildBillingAddressRecap();
        }

        [HttpPost]
        [Authorize]
        public ActionResult SelectShippingAddress(Guid shippingAddressId)
        {
            this.ShippingAddressId = shippingAddressId;
            this.SetAddresses(this.BillingAddressId.Value, this.ShippingAddressId);
            return this.BuildShippingAddressRecap();
        }

        [HttpGet]
        public ActionResult GetBillingAddressRecap()
        {
            this.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            return this.BuildRawBillingAddressRecap();
        }

        [HttpGet]
        public ActionResult GetShippingAddressRecap()
        {
            this.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            return this.BuildRawShippingAddressRecap();
        }

        [HttpPost]
        public ActionResult RegisterBillingAddress(AddressViewModel viewModel)
        {
            return this.RegisterAddress(this.RawBillingAddress, viewModel);
        }

        [HttpPost]
        public ActionResult RegisterShippingAddress(AddressViewModel viewModel)
        {
            return this.RegisterAddress(this.RawShippingAddress, viewModel);
        }

        [HttpGet]
        public ActionResult EditShippingRates()
        {
            Basket basket = null;
            ShippingRatesViewModel viewModel = new ShippingRatesViewModel();
            this.Initialize(viewModel);
            this._webStoreServices.UsingClient(c => basket = c.StoreClient.GetBasket("default"));
            viewModel.ShippingRateValueSelections = viewModel.ShippingRatesByPackage.Select(srbp => new ShippingRatesViewModel.ShippingRateValueSelection { PackageId = srbp.Key, ShippingRateValueId = srbp.Value.OrderByDescending(srv => basket != null && basket.Packages.Where(p => p.PackageId == srbp.Key).Any(p => p.ShippingRateCode.EqualsInvariantCultureIgnoreCase(srv.ShippingRateCode))).Select(srv => srv.ShippingRateValueId).Cast<Nullable<Guid>>().FirstOrDefault() }).ToList();
            this.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            return this.BuildShippingRatesShapeResult(viewModel);
        }

        [HttpPost]
        public ActionResult SetShippingRates(ShippingRatesViewModel viewModel)
        {
            if (this.ModelState.IsValid)
            {
                String basketHash = null;
                Exception exception = this._webStoreServices.UsingClient(c =>
                {
                    c.StoreClient.SetShippingRateValues("default", viewModel.ShippingRateValueSelections.ToDictionary(srvs => srvs.PackageId, srvs => srvs.ShippingRateValueId.Value));
                    basketHash = c.StoreClient.GetBasketHash("default");
                }
            );
                return this.Json(new { success = exception == null, basketHash = basketHash });
            }
            else
            {
                this.Initialize(viewModel);
                return this.BuildShippingRatesShapeResult(viewModel);
            }
        }

        [HttpGet]
        public ActionResult GetShippingRatesRecap()
        {
            ShippingRatesRecapViewModel viewModel = new ShippingRatesRecapViewModel();
            viewModel.NumberFormat = this._webStoreServices.NumberFormat;
            this._webStoreServices.UsingClient(
                c =>
                {
                    Dictionary<Guid, IEnumerable<ShippingRateValue>> shippingRatesByPackage = c.StoreClient.GetShippingRateValues("default");
                    viewModel.AddRange(c.StoreClient.GetBasket("default").Packages.Where(p => !p.Virtual).Select(p => shippingRatesByPackage.Where(srbp => srbp.Key == p.PackageId).SelectMany(srbp => srbp.Value).FirstOrDefault(srv => srv.ShippingRateCode.EqualsInvariantCultureIgnoreCase(p.ShippingRateCode))));
                }
            );
            this.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            return new ShapePartialResult(this, this._shapeFactory.DisplayTemplate(TemplateName: "Checkout/ShippingRatesRecap", Model: viewModel));
        }

        [HttpGet]
        public ActionResult EditPayment(String basketHash)
        {
            PaymentViewModel viewModel = new PaymentViewModel { BasketHash = basketHash };
            this.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            return this.BuildPaymentShape(viewModel);
        }

        [HttpPost]
        public ActionResult SaveAsOrder(PaymentViewModel viewModel)
        {
            if (this.ModelState.IsValid)
            {
                Order order = null;
                SaveAsOrderResult result = SaveAsOrderResult.Error;
                Exception exception = this._webStoreServices.UsingClient(c => order = c.StoreClient.SaveAsOrder("default", viewModel.BasketHash, this.Request.UserHostAddress, null, out result));
                if (exception != null)
                {
                    this.ModelState.AddModelError("UnexpectedError", this._localizer("An unexpected error has occured, please try again later").ToString());
                }
                else if (result == SaveAsOrderResult.Success)
                {
                    return this.BuildProceedToPaymentShape(new ProceedToPaymentViewModel { Order = order });
                }
                else
                {
                    switch (result)
                    {
                        case SaveAsOrderResult.AddressMissing:
                            this.ModelState.AddModelError("AddressMissing", this._localizer("An address is missing in your basket, please proceed to checkout again").ToString());
                            break;
                        case SaveAsOrderResult.BasketNotFound:
                            this.ModelState.AddModelError("BasketNotFound", this._localizer("Your basket is empty, it could has been emptied because products are not deliverable in your country").ToString());
                            break;
                        case SaveAsOrderResult.Error:
                            this.ModelState.AddModelError("UnexpectedError", this._localizer("An unexpected error has occured, please try again later").ToString());
                            break;
                        case SaveAsOrderResult.InconsistentBasket:
                            this.ModelState.AddModelError("InconsistentBasket", this._localizer("Your basket contains errors, please refresh your basket and proceed to checkout again").ToString());
                            break;
                        case SaveAsOrderResult.InvalidHash:
                            this.ModelState.AddModelError("InvalidHash", this._localizer("Your basket has changed during the checkout, please refresh your basket and proceed to checkout again").ToString());
                            break;
                        case SaveAsOrderResult.ProductNotFound:
                            this.ModelState.AddModelError("ProductNotFound", this._localizer("A product in your basket is no more available, please refresh your basket and proceed to checkout again").ToString());
                            break;
                        case SaveAsOrderResult.ShippingRateValueMissing:
                            this.ModelState.AddModelError("ShippingRateValueMissing", this._localizer("A package in your basket couldn't not be delivered, please refresh your basket and proceed to checkout again").ToString());
                            break;
                        default:
                            this.ModelState.AddModelError("UnexpectedError", this._localizer("An unexpected error has occured, please try again later").ToString());
                            break;
                    }
                }
            }
            return this.BuildPaymentShape(viewModel);
        }
    }
}