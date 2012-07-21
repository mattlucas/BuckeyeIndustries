using System;
using System.Collections.Specialized;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using Magelia.WebStore.Contracts;
using Magelia.WebStore.Extensions;
using Magelia.WebStore.Models.Parts;
using Magelia.WebStore.Models.ViewModels.CatalogHierarchy;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;

namespace Magelia.WebStore.Drivers
{
    public class CatalogHierarchyPartDriver : ContentPartDriver<CatalogHierarchyPart>
    {
        public class CatalogHierarchyTools
        {
            private CatalogHierarchyPart _catalogHieratchyPart;
            private ICatalogHierarchyServices _catalogHierarchyServices;

            private String GetValue(Object parent, String path)
            {
                Object element = parent;
                foreach (String property in path.Split('.'))
                {
                    PropertyInfo propertInfo;
                    if (element != null && (propertInfo = element.GetType().GetProperty(property)) != null)
                    {
                        element = propertInfo.GetValue(element, null);
                    }
                    else
                    {
                        return null;
                    }
                }
                return element.ToString();
            }

            private String GenerateUrl(String url, Object item)
            {
                MatchCollection matches = new Regex(@"\{([^\}]+)\}*").Matches(url);
                foreach (Match match in matches)
                {
                    String path = match.Groups[1].Value;
                    String value = this.GetValue(item, path);
                    url = url.Replace(match.Groups[0].Value, value);
                }
                return url;
            }

            public CatalogHierarchyTools(CatalogHierarchyPart catalogHierarchyPart, ICatalogHierarchyServices catalogHierarchyServices)
            {
                this._catalogHieratchyPart = catalogHierarchyPart;
                this._catalogHierarchyServices = catalogHierarchyServices;
            }

            public String GetExpandUrl(Boolean expanded, String path)
            {
                return HttpContext.Current.Request.Url.AddParameters(
                    new NameValueCollection { 
                    { this._catalogHierarchyServices.TargetParameterKey, this._catalogHieratchyPart.Id.ToString() }, 
                    { this._catalogHierarchyServices.ActionParameterKey, expanded ? this._catalogHierarchyServices.CollapseParameterValue : this._catalogHierarchyServices.ExpandParameterValue }, 
                    { this._catalogHierarchyServices.PathParameterKey, path } 
                }).ToString();
            }

            public String GenerateUrl(Object item)
            {
                String url = "~/";
                if (item is CatalogItemViewModel)
                {
                    url = this.GenerateUrl(this._catalogHieratchyPart.CatalogUrlPattern, (item as CatalogItemViewModel).Catalog);
                }
                else if (item is CategoryItemViewModel)
                {
                    url = this.GenerateUrl(this._catalogHieratchyPart.CategoryUrlPattern, (item as CategoryItemViewModel).Category);
                }
                return url;
            }

            public String GetSelectUrl(String path)
            {
                return HttpContext.Current.Request.Url.AddParameters(
                    new NameValueCollection { 
                    { this._catalogHierarchyServices.TargetParameterKey, this._catalogHieratchyPart.Id.ToString() }, 
                    { this._catalogHierarchyServices.ActionParameterKey, this._catalogHierarchyServices.SelectParameterValue }, 
                    { this._catalogHierarchyServices.PathParameterKey, path } 
                }).ToString();
            }
        }

        private Localizer _localizer { get; set; }
        private ICatalogHierarchyServices _catalogHierarchyServices;

        protected override String Prefix
        {
            get
            {
                return "Magelia_WebStore_CatalogHierarchy";
            }
        }

        protected override DriverResult Display(CatalogHierarchyPart part, String displayType, dynamic shapeHelper)
        {
            return this.ContentShape(
                "Parts_CatalogHierarchy",
                () => shapeHelper.Parts_CatalogHierarchy(
                    CatalogHierarchyPartId: part.Id,
                    GenerateUrls: part.GenerateUrls,
                    PathSeparator: this._catalogHierarchyServices.PathSeparator,
                    Hierarchy: this._catalogHierarchyServices.GetModel(part),
                    Tools: new CatalogHierarchyTools(part, this._catalogHierarchyServices)
                )
            );
        }

        protected override DriverResult Editor(CatalogHierarchyPart part, dynamic shapeHelper)
        {
            return this.ContentShape(
                "Parts_CatalogHierarchy_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/CatalogHierarchy",
                    Prefix: this.Prefix,
                    Model: part
                )
            );
        }

        protected override DriverResult Editor(CatalogHierarchyPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, this.Prefix, null, null);
            if (part.GenerateUrls && String.IsNullOrEmpty(part.CategoryUrlPattern))
            {
                updater.AddModelError("CategoryUrlPattern", this._localizer("Category url pattern is required"));
            }
            return this.Editor(part, shapeHelper);
        }

        public CatalogHierarchyPartDriver(ICatalogHierarchyServices catalogHierarchyServices)
        {
            this._localizer = NullLocalizer.Instance;
            this._catalogHierarchyServices = catalogHierarchyServices;
        }
    }
}