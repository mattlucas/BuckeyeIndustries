using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Magelia.WebStore.Client;
using Magelia.WebStore.Contracts;
using Magelia.WebStore.Models.Parts;
using Magelia.WebStore.Models.ViewModels.User;
using Magelia.WebStore.Services.Contract.Data.Store;
using Orchard;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Mvc;
using Customer = Magelia.WebStore.Services.Contract.Data.Customer;

namespace Magelia.WebStore.Controllers
{
    public class UserController : Controller
    {
        private IUserOrdersServices _userOrdersServices;
        private IWebStoreServices _webStoreServices;
        private IOrchardServices _orchardServices;
        private dynamic _shapeFactory;
        private Localizer _localizer;

        private AddressViewModel Initialize(AddressViewModel viewModel)
        {
            if (viewModel != null)
            {
                viewModel.Countries = this._webStoreServices.StoreContext.AvailableCountries.Select(ac => new SelectListItem { Text = ac.Name, Value = ac.CountryId.ToString() });
                if (viewModel.CountryId.HasValue)
                {
                    this._webStoreServices.UsingClient(c => viewModel.Regions = c.StoreClient.GetRegions(viewModel.CountryId.Value).Select(r => new SelectListItem { Text = r.Name, Value = r.RegionId.ToString() }));
                }
            }
            return viewModel;
        }

        private Customer.Address GetAddress(AddressViewModel viewModel)
        {
            return new Customer.Address
            {
                AddressId = viewModel.AddressId.HasValue ? viewModel.AddressId.Value : Guid.NewGuid(),
                City = viewModel.City,
                Company = viewModel.Company,
                CountryId = viewModel.CountryId.Value,
                DigiCode = viewModel.DigiCode,
                Email = viewModel.Email,
                FaxNumber = viewModel.FaxNumber,
                FirstName = viewModel.FirstName,
                Floor = viewModel.Floor,
                LastName = viewModel.LastName,
                Line1 = viewModel.Line1,
                Line2 = viewModel.Line2,
                Line3 = viewModel.Line3,
                MiddleName = viewModel.MiddleName,
                MobileNumber = viewModel.MobileNumber,
                Name = viewModel.Name,
                PhoneNumber = viewModel.PhoneNumber,
                RegionId = viewModel.RegionId,
                ZipCode = viewModel.ZipCode
            };
        }

        private OrderAddressViewModel GetAddress(OrderAddress address, IEnumerable<Country> countries, WebStoreClient client)
        {
            if (address != null)
            {
                IEnumerable<Region> regions = address.RegionId.HasValue ? client.StoreClient.GetRegions(address.CountryId) : Enumerable.Empty<Region>();
                return new OrderAddressViewModel
                {
                    City = address.CityName,
                    Company = address.CompanyName,
                    CountryName = countries.Where(c => c.CountryId == address.CountryId).Select(c => c.Name).FirstOrDefault(),
                    DigiCode = address.DigiCode,
                    Email = address.EmailAddress,
                    FaxNumber = address.FaxNumber,
                    FirstName = address.FirstName,
                    Floor = address.FloorNumber,
                    LastName = address.LastName,
                    Line1 = address.Address1,
                    Line2 = address.Address2,
                    Line3 = address.Address3,
                    MiddleName = address.MiddleName,
                    MobileNumber = address.MobileNumber,
                    Name = address.Name,
                    PhoneNumber = address.PhoneNumber,
                    RegionName = regions.Where(r => r.RegionId == address.RegionId).Select(r => r.Name).FirstOrDefault(),
                    ZipCode = address.ZipCode
                };
            }
            return null;
        }

        public UserController(IWebStoreServices webStoreServices, IOrchardServices orchardServices, IShapeFactory shapeFactory, IUserOrdersServices userOrdersServices)
        {
            this._userOrdersServices = userOrdersServices;
            this._webStoreServices = webStoreServices;
            this._orchardServices = orchardServices;
            this._shapeFactory = shapeFactory;
            this._localizer = NullLocalizer.Instance;
        }

        private ShapePartialResult BuildAddressShape(AddressViewModel viewModel)
        {
            return new ShapePartialResult(this, this._shapeFactory.EditorTemplate(TemplateName: "User/Address", Model: this.Initialize(viewModel)));
        }

        [HttpGet]
        [Authorize]
        public JsonResult GetAddresses()
        {
            Object result = null;
            this._webStoreServices.UsingClient(c => result = c.CustomerClient.GetAddresses(this._orchardServices.WorkContext.CurrentUser.UserName).Select(a => new { name = a.Name, addressId = a.AddressId }));
            this.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            return this.Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Authorize]
        public ActionResult NewAddress()
        {
            this.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            return this.BuildAddressShape(new AddressViewModel { Named = true });
        }

        [HttpPost]
        [Authorize]
        public ActionResult SaveAddress(AddressViewModel viewModel)
        {
            if (this.ModelState.IsValid)
            {
                Boolean success = true;
                this._webStoreServices.UsingClient(
                    c =>
                    {
                        if (viewModel.RegionId.HasValue || (!viewModel.RegionId.HasValue && !c.StoreClient.GetRegions(viewModel.CountryId.Value).Any()))
                        {
                            List<Customer.Address> addresses = c.CustomerClient.GetAddresses(this._orchardServices.WorkContext.CurrentUser.UserName).ToList();
                            if (addresses.Any(a => a.Name.Equals(viewModel.Name, StringComparison.InvariantCulture) && a.AddressId != viewModel.AddressId))
                            {
                                success = false;
                                this.ModelState.AddModelError("DuplicatedName", this._localizer("Address name already used").ToString());
                            }
                            else
                            {
                                addresses.Remove(addresses.FirstOrDefault(a => a.AddressId == viewModel.AddressId));
                                addresses.Add(this.GetAddress(viewModel));
                                c.CustomerClient.UpdateAddresses(this._orchardServices.WorkContext.CurrentUser.UserName, addresses);
                            }
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
                    return this.Json(new { name = viewModel.Name, shippingAddressIsDifferent = viewModel.ShippingAddressIsDifferent });
                }
            }
            return this.BuildAddressShape(viewModel);
        }

        [HttpGet]
        [Authorize]
        public PartialViewResult GetAddress(Guid addressId, Nullable<Boolean> promptShippingAddressIsDifferent, Nullable<Boolean> shippingAddressIsDifferent)
        {
            AddressViewModel viewModel = null;
            this._webStoreServices.UsingClient(
                c =>
                {
                    Customer.Address address = c.CustomerClient.GetAddresses(this._orchardServices.WorkContext.CurrentUser.UserName).FirstOrDefault(a => a.AddressId == addressId);
                    if (address != null)
                    {
                        viewModel = new AddressViewModel
                        {
                            PromptShippingAddressIsDifferent = promptShippingAddressIsDifferent.HasValue && promptShippingAddressIsDifferent.Value,
                            ShippingAddressIsDifferent = shippingAddressIsDifferent.HasValue && shippingAddressIsDifferent.Value,
                            Named = true,
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
                            Name = address.Name,
                            PhoneNumber = address.PhoneNumber,
                            RegionId = address.RegionId,
                            ZipCode = address.ZipCode
                        };
                    }
                }
            );
            this.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            return this.BuildAddressShape(viewModel);
        }

        [HttpDelete]
        [Authorize]
        public void DeleteAddress(Guid addressId)
        {
            this._webStoreServices.UsingClient(c =>
                {
                    var t1 = c.CustomerClient.GetAddresses(this._orchardServices.WorkContext.CurrentUser.UserName);
                    var t2 = t1.Where(a => a.AddressId != addressId).ToList();
                    c.CustomerClient.UpdateAddresses(this._orchardServices.WorkContext.CurrentUser.UserName, t2);
                }
            );
        }

        [HttpGet]
        [Authorize]
        public ActionResult GetOrders(Int32 userOrdersPartId, Nullable<OrderSortDirection> sortDirection, Nullable<OrderSortExpression> sortExpression, Nullable<Int32> page)
        {
            UserOrdersPart userOrdersPart = this._orchardServices.ContentManager.Get<UserOrdersPart>(userOrdersPartId, VersionOptions.Published);
            this._userOrdersServices.UpdateState(userOrdersPart, sortDirection, sortExpression, page);
            this.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            return new ShapePartialResult(this, this._orchardServices.ContentManager.BuildDisplay(userOrdersPart));
        }

        [HttpGet]
        [Authorize]
        public ActionResult GetOrder(Int32 userOrdersPartId, Guid orderId)
        {
            OrderViewModel viewModel = new OrderViewModel { UserOrdersPartId = userOrdersPartId, NumberFormat = CultureInfo.GetCultureInfo(this._orchardServices.WorkContext.CurrentCulture).NumberFormat.Clone() as NumberFormatInfo };
            this._webStoreServices.UsingClient(
                c =>
                {
                    IEnumerable<Country> counrtries = c.StoreClient.GetAllCountries();
                    viewModel.Order = c.StoreClient.GetOrder(orderId);
                    viewModel.BillingAddress = this.GetAddress(viewModel.Order.BillingAddress, counrtries, c);
                    viewModel.ShippingAddress = this.GetAddress(viewModel.Order.ShippingAddress, counrtries, c);
                }
            );
            viewModel.NumberFormat.CurrencySymbol = this._webStoreServices.StoreContext.AvailableCurrencies.Where(ac => ac.CurrencyId == viewModel.Order.CurrencyId).Select(ac => ac.Symbol).FirstOrDefault();
            this.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            return new ShapePartialResult(this, this._shapeFactory.DisplayTemplate(TemplateName: "User/Order", Model: viewModel));
        }
    }
}