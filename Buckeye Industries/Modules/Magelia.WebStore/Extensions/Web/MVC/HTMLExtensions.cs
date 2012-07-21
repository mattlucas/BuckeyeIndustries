using System;
using System.Web.Mvc;

namespace Magelia.WebStore.Extensions.Web.MVC
{
    public static class HTMLExtensions
    {
        public static String RandomId(this HtmlHelper helper)
        {
            return String.Format("_{0}", Guid.NewGuid());
        }
    }
}