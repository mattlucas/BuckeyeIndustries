using System;
using System.Globalization;
using Magelia.WebStore.Contracts;
using Magelia.WebStore.Services.Contract.Parameters.Store;
using Orchard;
using Orchard.Security;
using Orchard.Users.Events;

namespace Magelia.WebStore.Handlers
{
    public class UserEventHandler : IUserEventHandler
    {
        private IUserModelsStateServices _userModelsStateServices;
        private IWebStoreServices _webStoreServices;
        private IOrchardServices _orchardServices;

        private void EnsureUser(IUser user)
        {
            this._webStoreServices.EnsureUser(user);
        }

        private void TransfertAndUpdateBasket(String fromUserName, Boolean isAnonymous, String newUsername)
        {
            this._webStoreServices.UsingClient(
                c =>
                {
                    c.Settings.UserName = fromUserName;
                    c.Settings.IsAnonymous = isAnonymous;
                    c.StoreClient.TransferBaskets(newUsername, new[] { "default" });
                    c.StoreClient.UpdateBasket("default", newUsername, this._webStoreServices.CurrentCurrencyId, CultureInfo.GetCultureInfo(this._orchardServices.WorkContext.CurrentCulture).LCID, new Location { CountryId = this._webStoreServices.CurrentCountryId, RegionId = this._webStoreServices.CurrentRegionId });
                }
            );
        }

        public UserEventHandler(IWebStoreServices webStoreServices, IOrchardServices orchardServices, IUserModelsStateServices userModelsStateServices)
        {
            this._userModelsStateServices = userModelsStateServices;
            this._webStoreServices = webStoreServices;
            this._orchardServices = orchardServices;
        }

        public void Creating(UserContext context)
        {

        }

        public void Created(UserContext context)
        {

        }

        public void LoggedIn(IUser user)
        {
            this.EnsureUser(user);
            this._userModelsStateServices.FlushUserContext();
            this.TransfertAndUpdateBasket(this._webStoreServices.AnonymousUserName, true, user.UserName);
        }

        public void LoggedOut(IUser user)
        {
            this._userModelsStateServices.FlushUserContext();
            this.TransfertAndUpdateBasket(user.UserName, false, this._webStoreServices.AnonymousUserName);
        }

        public void AccessDenied(IUser user)
        {

        }

        public void ChangedPassword(IUser user)
        {

        }

        public void SentChallengeEmail(IUser user)
        {

        }

        public void ConfirmedEmail(IUser user)
        {

        }

        public void Approved(IUser user)
        {

        }
    }
}