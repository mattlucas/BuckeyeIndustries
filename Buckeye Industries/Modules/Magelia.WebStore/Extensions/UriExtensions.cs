using System;
using System.Collections.Specialized;
using System.Linq;
using System.Web;

namespace Magelia.WebStore.Extensions
{
    public static class UriExtensions
    {
        private const String ParameterPrefix = "x_";

        private static String FormatParameterKey(String key)
        {
            return String.Concat(UriExtensions.ParameterPrefix, key);
        }

        public static Uri Update(this Uri uri, NameValueCollection parameters)
        {
            return new Uri(String.Concat(uri.GetLeftPart(UriPartial.Authority), uri.AbsolutePath, parameters.Count > 0 ? "?" : String.Empty, parameters));
        }

        public static NameValueCollection GetParameters(this Uri uri)
        {
            return HttpUtility.ParseQueryString(uri.Query);
        }

        public static Uri RemoveAddedParameters(this Uri uri)
        {
            NameValueCollection parameters = uri.GetParameters();
            parameters.AllKeys.Where(k => k.StartsWith(UriExtensions.ParameterPrefix, StringComparison.InvariantCulture)).ToList().ForEach(k => parameters.Remove(k));
            return uri.Update(parameters);
        }

        public static Uri AddParameters(this Uri uri, NameValueCollection parameters)
        {
            NameValueCollection currentParameters = uri.RemoveAddedParameters().GetParameters();
            foreach (String key in parameters)
            {
                currentParameters.Add(UriExtensions.FormatParameterKey(key), parameters[key]);
            }
            return uri.Update(currentParameters);
        }

        public static String GetAddedParameter(this Uri uri, String key)
        {
            NameValueCollection parameters = uri.GetParameters();
            String formatedKey = UriExtensions.FormatParameterKey(key);
            String existingKey = parameters.AllKeys.FirstOrDefault(k => k.EqualsInvariantCultureIgnoreCase(formatedKey));
            return String.IsNullOrEmpty(existingKey) ? null : parameters[existingKey];
        }
    }
}