using System;
using System.Collections.Specialized;
using System.Web.Mvc;

namespace Magelia.WebStore.Extensions.Web.MVC
{
    public static class UrlExtensions
    {
        public static String AppendToCurrentUrl(this UrlHelper urlHelper, NameValueCollection parameters)
        {
            return urlHelper.RequestContext.HttpContext.Request.Url.AddParameters(parameters).ToString();
        }
    }
}
