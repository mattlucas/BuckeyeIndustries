using System;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Security;
using Magelia.WebStore.Client;
using Magelia.WebStore.Contracts;
using Magelia.WebStore.Extensions;
using Magelia.WebStore.Models.Parts;
using Magelia.WebStore.Services.Contract.Data.Store;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Logging;
using Orchard.Security;

namespace Magelia.WebStore.Services
{
    public class WebStoreServices : IWebStoreServices
    {
        private const String StoreContextHttpCacheKey = "storecontext";
        private const String CurrentCountryIdSessionKey = "currentcountryid";
        private const String CurrentRegionIdSessionKey = "currentregionid";
        private const String CurrentCurrencyIdSessionKey = "currentcurrencyid";
        private const String AnonymousUserNameCookieKey = "mageliaanonymoususername";

        private IUserModelsStateServices _userModelsStateServices;
        private IOrchardServices _orchardServices;
        private NumberFormatInfo _numberFormat;
        private ILogger _logger;

        private Nullable<Int32> _currentCountryId
        {
            get
            {
                return this._orchardServices.WorkContext.HttpContext.Session[WebStoreServices.CurrentCountryIdSessionKey] as Nullable<Int32>;
            }
            set
            {
                this._orchardServices.WorkContext.HttpContext.Session[WebStoreServices.CurrentCountryIdSessionKey] = value;
            }
        }

        private Nullable<Guid> _currentRegionId
        {
            get
            {
                return this._webStoreSettings.AllowRegionNavigation ? this._orchardServices.WorkContext.HttpContext.Session[WebStoreServices.CurrentRegionIdSessionKey] as Nullable<Guid> : null;
            }
            set
            {
                this._orchardServices.WorkContext.HttpContext.Session[WebStoreServices.CurrentRegionIdSessionKey] = value;
            }
        }

        private Nullable<Int32> _currentCurrencyId
        {
            get
            {
                return this._orchardServices.WorkContext.HttpContext.Session[WebStoreServices.CurrentCurrencyIdSessionKey] as Nullable<Int32>;
            }
            set
            {
                this._orchardServices.WorkContext.HttpContext.Session[WebStoreServices.CurrentCurrencyIdSessionKey] = value;
            }
        }

        private SettingsPart _webStoreSettings
        {
            get
            {
                return this._orchardServices.WorkContext.CurrentSite.As<SettingsPart>();
            }
        }

        private String _anonymousUserName;

        public NumberFormatInfo NumberFormat
        {
            get
            {
                if (this._numberFormat == null)
                {
                    this._numberFormat = this.GetNumberFormat();
                }
                return this._numberFormat;
            }
        }

        public StoreContext StoreContext
        {
            get
            {
                StoreContext defaultStoreContext = this.GetStoreContext(null);
                Culture culture = this.GetCorrespondingCulture(defaultStoreContext);
                return culture == null ? defaultStoreContext : this.GetStoreContext(new CultureInfo(this._orchardServices.WorkContext.CurrentCulture).LCID);
            }
        }

        public String AnonymousUserName
        {
            get
            {
                if (String.IsNullOrEmpty(this._anonymousUserName))
                {
                    this._anonymousUserName = this.EnsureAnonymousUserName();
                }
                return this._anonymousUserName;
            }
        }

        public String CurrentUserName
        {
            get
            {
                return this._orchardServices.WorkContext.CurrentUser == null ? this.AnonymousUserName : this._orchardServices.WorkContext.CurrentUser.UserName;
            }
        }

        public Int32 CurrentCountryId
        {
            get
            {
                if (!this.StoreContext.AvailableCountries.Any(ac => ac.CountryId == this._currentCountryId))
                {
                    this._currentCountryId = this.StoreContext.AvailableCountries.Where(ac => ac.IsDefault).Select(ac => ac.CountryId).FirstOrDefault();
                }
                return this._currentCountryId.Value;
            }
            set
            {
                if (this._currentCountryId != value)
                {
                    this._currentCountryId = value;
                    this._currentRegionId = null;
                    this._userModelsStateServices.FlushCommerceContext();
                }
            }
        }

        public Nullable<Guid> CurrentRegionId
        {
            get
            {
                return this._currentRegionId;
            }
            set
            {
                this._currentRegionId = value;
                this._userModelsStateServices.FlushCommerceContext();
            }
        }

        public Int32 CurrentCurrencyId
        {
            get
            {
                if (!this.StoreContext.AvailableCurrencies.Any(ac => ac.CurrencyId == this._currentCurrencyId))
                {
                    this._currentCurrencyId = this.StoreContext.AvailableCurrencies.Where(ac => ac.IsDefault).Select(ac => ac.CurrencyId).FirstOrDefault();
                }
                return this._currentCurrencyId.Value;
            }
            set
            {
                if (this._currentCurrencyId != value)
                {
                    this._currentCurrencyId = value;
                    this._userModelsStateServices.FlushCommerceContext();
                }
            }
        }

        private String EnsureAnonymousUserName()
        {
            HttpCookie cookie;
            if (HttpContext.Current.Request.Cookies.AllKeys.Contains(WebStoreServices.AnonymousUserNameCookieKey))
            {
                cookie = HttpContext.Current.Request.Cookies[WebStoreServices.AnonymousUserNameCookieKey];
            }
            else
            {
                cookie = new HttpCookie(WebStoreServices.AnonymousUserNameCookieKey, Guid.NewGuid().ToString());
                HttpContext.Current.Response.Cookies.Add(cookie);
            }
            cookie.Expires = DateTime.Now.AddYears(1);
            return cookie.Value;
        }

        private NumberFormatInfo GetNumberFormat()
        {
            NumberFormatInfo numberFormat = CultureInfo.GetCultureInfo(this._orchardServices.WorkContext.CurrentCulture).NumberFormat.Clone() as NumberFormatInfo;
            numberFormat.CurrencySymbol = this.StoreContext.AvailableCurrencies.Where(ac => ac.CurrencyId == this.CurrentCurrencyId).Select(c => c.Symbol).FirstOrDefault();
            return numberFormat;
        }

        private Culture GetCorrespondingCulture(StoreContext storeContext)
        {
            return storeContext.AvailableCultures.FirstOrDefault(ac => ac.NetName.EqualsInvariantCultureIgnoreCase(this._orchardServices.WorkContext.CurrentCulture));
        }

        private StoreContext GetStoreContext(Nullable<Int32> cultureId)
        {
            String storeContextCacheKey = String.Format("{0}-{1}", WebStoreServices.StoreContextHttpCacheKey, cultureId);
            StoreContext storeContext = HttpContext.Current.Cache.Get(storeContextCacheKey) as StoreContext;
            if (storeContext == null)
            {
                this.Execute(
                    () => this.NewClient(false),
                    c =>
                    {
                        storeContext = c.StoreClient.GetContext();
                        HttpContext.Current.Cache.Add(
                            storeContextCacheKey,
                            storeContext,
                            null,
                            DateTime.Now.AddHours(1),
                            Cache.NoSlidingExpiration,
                            CacheItemPriority.Normal,
                            null
                        );
                    }
                );
            }
            return storeContext;
        }

        private WebStoreClient NewClient(Boolean contextualize)
        {
            return this.NewClient(this._webStoreSettings.StoreId, this._webStoreSettings.ServicesPath, contextualize);
        }

        private WebStoreClient NewClient(Guid storeId, String servicesPath, Boolean contextualize)
        {
            WebStoreClient client = new WebStoreClient(new WebStoreClientSettings(storeId, new Uri(servicesPath)));
            if (contextualize)
            {
                client.Settings.IsAnonymous = this._orchardServices.WorkContext.CurrentUser == null;
                client.Settings.UserName = client.Settings.IsAnonymous ? this.AnonymousUserName : this._orchardServices.WorkContext.CurrentUser.UserName;
                client.Context = new WebStoreClientContext { CountryId = this._currentCountryId, RegionId = this._currentRegionId, CurrencyId = this._currentCurrencyId };
                Culture culture = this.GetCorrespondingCulture(this.StoreContext);
                if (culture != null)
                {
                    client.Context.CultureId = culture.LCID;
                }
            }
            return client;
        }

        private Exception Execute(Func<WebStoreClient> clientBuilder, Action<WebStoreClient> action)
        {
            try
            {
                using (WebStoreClient client = clientBuilder())
                {
                    action(client);
                }
            }
            catch (Exception e)
            {
                this._logger.Error(e, null);
                return e;
            }
            return null;
        }

        public WebStoreServices(IOrchardServices orchardServices, IUserModelsStateServices userModelsStateServices)
        {
            this._userModelsStateServices = userModelsStateServices;
            this._orchardServices = orchardServices;
            this._logger = NullLogger.Instance;
        }

        public Exception UsingClient(Guid storeId, String servicesPath, Action<WebStoreClient> action)
        {
            return this.Execute(() => this.NewClient(storeId, servicesPath, false), action);
        }

        public Exception UsingClient(Action<WebStoreClient> action)
        {
            return this.Execute(() => this.NewClient(true), action);
        }

        public void EnsureUser(IUser user)
        {
            this.UsingClient(
                c =>
                {
                    if (c.CustomerClient.GetCustomer(user.UserName, true) == null)
                    {
                        MembershipCreateStatus status;
                        c.CustomerClient.CreateCustomer(user.UserName, Guid.NewGuid().ToString(), String.IsNullOrEmpty(user.Email) ? null : user.Email, null, null, true, out status);
                    }
                }
            );
        }
    }
}