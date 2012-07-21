using System;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using Magelia.WebStore.Contracts;

namespace Magelia.WebStore.Services
{
    public class UserModelsStateServices : IUserModelsStateServices
    {
        private const String KeySeparator = "-";
        private const String CommerceContextPrefix = "commercecontext";
        private const String UserContextPrefix = "usercontext";

        private HttpSessionState _session
        {
            get
            {
                return HttpContext.Current.Session;
            }
        }

        private String JoinKeys(params Object[] fragments)
        {
            return String.Join(UserModelsStateServices.KeySeparator, fragments);
        }

        private T Get<T>(String key)
            where T : class, new()
        {
            if (this._session[key] == null)
            {
                this._session[key] = Activator.CreateInstance<T>();
            }
            return this._session[key] as T;
        }

        private void Flush(String prefix)
        {
            this._session.Keys.Cast<String>().Where(k => k.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase)).ToList().ForEach(k => this._session.Remove(k));
        }

        public T GetFromCommerceContext<T>(String type, Int32 id)
            where T : class, new()
        {
            return this.Get<T>(this.JoinKeys(UserModelsStateServices.CommerceContextPrefix, type, id));
        }

        public T GetFromUserContext<T>(String type, Int32 id)
            where T : class, new()
        {
            return this.Get<T>(this.JoinKeys(UserModelsStateServices.UserContextPrefix, type, id));
        }

        public void FlushCommerceContext()
        {
            this.Flush(this.JoinKeys(UserModelsStateServices.CommerceContextPrefix));
        }

        public void FlushUserContext()
        {
            this.Flush(this.JoinKeys(UserModelsStateServices.UserContextPrefix));
        }
    }
}