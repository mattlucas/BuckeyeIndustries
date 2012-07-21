using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.UI.WebControls;
using Magelia.WebStore.Client;
using Magelia.WebStore.Contracts;
using Magelia.WebStore.Extensions;
using Magelia.WebStore.Models.Parts;
using Magelia.WebStore.Models.ViewModels.CatalogHierarchy;
using Magelia.WebStore.Models.ViewModels.ProductList;
using Orchard;
using Orchard.ContentManagement;

namespace Magelia.WebStore.Services
{
    public class ProductListServices : IProductListServices
    {
        private const String ProductListUserModelsStateCategory = "productlist";

        private IWebStoreServices _webStoreServices;
        private IOrchardServices _orchardServices;
        private IUserModelsStateServices _userModelsStateServices;
        private ICatalogHierarchyServices _catalogHierarchyServices;

        private Nullable<Int32> _page
        {
            get
            {
                Int32 page;
                if (Int32.TryParse(HttpContext.Current.Request.Url.GetAddedParameter(this.PageParameterKey), out page))
                {
                    return page;
                }
                return null;
            }
        }

        private Nullable<SortDirection> _sortDirection
        {
            get
            {
                SortDirection sortDirection;
                if (Enum.TryParse(HttpContext.Current.Request.Url.GetAddedParameter(this.SortDirectionParameterKey), out sortDirection))
                {
                    return sortDirection;
                }
                return null;
            }
        }

        private String _sortExpression
        {
            get
            {
                return HttpContext.Current.Request.Url.GetAddedParameter(this.SortExpressionParameterKey);
            }
        }

        private Nullable<Int32> _target
        {
            get
            {
                Int32 target;
                if (Int32.TryParse(HttpContext.Current.Request.Url.GetAddedParameter(this.TargetParameterKey), out target))
                {
                    return target;
                }
                return null;
            }
        }

        public String TargetParameterKey
        {
            get
            {
                return "plw_target";
            }
        }

        public String PageParameterKey
        {
            get
            {
                return "plw_page";
            }
        }

        public String SortDirectionParameterKey
        {
            get
            {
                return "plw_direction";
            }
        }

        public String SortExpressionParameterKey
        {
            get
            {
                return "plw_sort";
            }
        }

        private ProductListViewModel.ProductListState GetState(ProductListPart part)
        {
            return this._userModelsStateServices.GetFromCommerceContext<ProductListViewModel.ProductListState>(ProductListServices.ProductListUserModelsStateCategory, part.Id);
        }

        private HierarchyItemViewModel GetSelected(IEnumerable<HierarchyItemViewModel> items)
        {
            foreach (HierarchyItemViewModel item in items)
            {
                HierarchyItemViewModel subSelectedItem;
                if (item.Selected)
                {
                    return item;
                }
                else if ((subSelectedItem = this.GetSelected(item.Categories)) != null)
                {
                    return subSelectedItem;
                }
            }
            return null;
        }

        private Boolean Contains(IEnumerable<CategoryItemViewModel> categories, CategoryItemViewModel searchCategory)
        {
            foreach (CategoryItemViewModel category in categories)
            {
                if (category == searchCategory || this.Contains(category.Categories, searchCategory))
                {
                    return true;
                }
            }
            return false;
        }

        private CatalogItemViewModel GetCatalog(HierarchyViewModel hierarchy, CategoryItemViewModel category)
        {
            return hierarchy.FirstOrDefault(cata => this.Contains(cata.Categories, category));
        }

        private String GetPropertyName<T, P>(Expression<Func<T, P>> accessor)
        {
            return (accessor.Body as MemberExpression).Member.Name;
        }

        private void UpdateState(ProductListPart part, ProductListViewModel viewModel)
        {
            String previousCatalogCodeFilter = viewModel.State.CatalogCodeFilter ?? String.Empty;
            String previousCategoryCodeFilter = viewModel.State.CategoryCodeFilter ?? String.Empty;
            if (part.FromCatalogHierarchySelection)
            {
                HierarchyViewModel hierarchy;
                HierarchyItemViewModel selectedItem;
                CatalogHierarchyPart catalogHierarchyPart;
                if (
                    part.CatalogHierarchyId.HasValue &&
                    (catalogHierarchyPart = this._orchardServices.ContentManager.Get<CatalogHierarchyPart>(part.CatalogHierarchyId.Value, VersionOptions.Published)) != null &&
                    !catalogHierarchyPart.GenerateUrls &&
                    (hierarchy = this._catalogHierarchyServices.GetModel(catalogHierarchyPart)) != null &&
                    (selectedItem = this.GetSelected(hierarchy)) != null
                )
                {
                    if (selectedItem is CatalogItemViewModel)
                    {
                        CatalogItemViewModel catalog = selectedItem as CatalogItemViewModel;
                        viewModel.State.CatalogCodeFilter = catalog.Catalog.Code;
                        viewModel.State.CategoryCodeFilter = null;
                    }
                    else if (selectedItem is CategoryItemViewModel)
                    {
                        CategoryItemViewModel category = selectedItem as CategoryItemViewModel;
                        viewModel.State.CatalogCodeFilter = this.GetCatalog(hierarchy, category).Catalog.Code;
                        viewModel.State.CategoryCodeFilter = category.Category.Code;
                    }
                    viewModel.State.FromPath = this.GetPath(hierarchy, selectedItem);
                }
                else
                {
                    viewModel.State.CatalogCodeFilter = null;
                    viewModel.State.CategoryCodeFilter = null;
                    viewModel.State.FromPath = null;
                }
            }
            else
            {
                viewModel.State.CatalogCodeFilter = part.CatalogCodeFilter;
                viewModel.State.CategoryCodeFilter = part.CategoryCodeFilter;
            }
            if (this._target == part.Id)
            {
                viewModel.State.Page = this._page;
                viewModel.State.SortDirection = this._sortDirection;
                viewModel.State.SortExpression = this._sortExpression;
            }
            Boolean hasSortExpression = !String.IsNullOrEmpty(viewModel.State.SortExpression);
            Boolean hasCategoryFilter = !String.IsNullOrEmpty(viewModel.State.CategoryCodeFilter);
            Boolean hasCatalogFilter = !String.IsNullOrEmpty(viewModel.State.CatalogCodeFilter);
            if (part.EnableSorting)
            {
                if (!viewModel.State.SortDirection.HasValue)
                {
                    viewModel.State.SortDirection = SortDirection.Ascending;
                }
                if (!hasSortExpression)
                {
                    viewModel.State.SortExpression = hasCategoryFilter ? "custom" : this.GetPropertyName<BaseProduct, String>(bp => bp.Name);
                }
                else if ("custom".EqualsInvariantCultureIgnoreCase(viewModel.State.SortExpression) && !hasCategoryFilter)
                {
                    viewModel.State.SortExpression = this.GetPropertyName<BaseProduct, String>(bp => bp.Name);
                }
            }
            else
            {
                viewModel.State.SortDirection = SortDirection.Ascending;
                viewModel.State.SortExpression = String.IsNullOrEmpty(viewModel.State.CategoryCodeFilter) ? this.GetPropertyName<BaseProduct, String>(bp => bp.Name) : "custom";
            }
            if (
                (!previousCatalogCodeFilter.EqualsInvariantCultureIgnoreCase(viewModel.State.CatalogCodeFilter ?? String.Empty) && !this._page.HasValue) ||
                (!previousCategoryCodeFilter.EqualsInvariantCultureIgnoreCase(viewModel.State.CategoryCodeFilter ?? String.Empty) && !this._page.HasValue) ||
                !part.EnablePaging ||
                viewModel.State.Page < 1 ||
                !viewModel.State.Page.HasValue
            )
            {
                viewModel.State.Page = 1;
            }
        }

        private String GetPath(IEnumerable<HierarchyItemViewModel> items, HierarchyItemViewModel selectedItem)
        {
            HierarchyItemViewModel parentItem;
            if (items.Contains(selectedItem))
            {
                return this.GetCode(selectedItem);
            }
            else if (selectedItem is CategoryItemViewModel && (parentItem = items.FirstOrDefault(i => this.Contains(i.Categories, selectedItem as CategoryItemViewModel))) != null)
            {
                return String.Concat(this.GetCode(parentItem), this._catalogHierarchyServices.PathSeparator, this.GetPath(parentItem.Categories, selectedItem));
            }
            return null;
        }

        private String GetCode(HierarchyItemViewModel item)
        {
            if (item is CatalogItemViewModel)
            {
                return (item as CatalogItemViewModel).Catalog.Code;
            }
            else if (item is CategoryItemViewModel)
            {
                return (item as CategoryItemViewModel).Category.Code;
            }
            return null;
        }

        private Expression<Func<T, Object>> GetMemberAccesExpression<T>(String expression)
        {
            ParameterExpression parameter = Expression.Parameter(typeof(T));
            Expression accessor = null;
            foreach (String property in expression.Split('.'))
            {
                accessor = Expression.Property(accessor ?? parameter, property);
            }
            return Expression.Lambda<Func<T, Object>>(Expression.Convert(accessor, typeof(Object)), parameter);
        }

        private void LoadProducts(ProductListPart part, ProductListViewModel viewModel)
        {
            viewModel.Clear();
            this._webStoreServices.UsingClient(
                c =>
                {
                    Int32 skip = (part.PageSize ?? 0) * ((viewModel.State.Page ?? 1) - 1);
                    Int32 take = part.EnablePaging && part.PageSize.HasValue ? part.PageSize.Value : Int32.MaxValue;
                    if ("custom".EqualsInvariantCultureIgnoreCase(viewModel.State.SortExpression))
                    {
                        IQueryable<CategoryProduct> categoryProductsQuery = c.CatalogClient.CategoryProducts
                                                                                                .Expand(cp => cp.Product.Brand)
                                                                                                .Expand(cp => cp.Product.Catalog)
                                                                                                .Expand(String.Format("Product/{0}/Attributes/Files", c.CatalogClient.ResolveName(typeof(ReferenceProduct))))
                                                                                                .Expand(cp => (cp.Product as VariableProduct).DefaultVariantProduct.PriceWithLowerQuantity)
                                                                                                .Expand(cp => (cp.Product as VariableProduct).DefaultVariantProduct.Brand)
                                                                                                .Expand(String.Format("Product/{0}/DefaultVariantProduct/Attributes/Files", c.CatalogClient.ResolveName(typeof(VariableProduct))))
                                                                                                .Expand(cp => (cp.Product as ReferenceProduct).PriceWithLowerQuantity)
                                                                                                .Where(cp => cp.Category.Code == viewModel.State.CategoryCodeFilter);
                        if (!String.IsNullOrEmpty(viewModel.State.CatalogCodeFilter))
                        {
                            categoryProductsQuery = categoryProductsQuery.Where(cp => cp.Category.Catalog.Code == viewModel.State.CatalogCodeFilter);
                        }
                        viewModel.AddRange((viewModel.State.SortDirection == SortDirection.Descending ? categoryProductsQuery.OrderByDescending(cp => cp.Order) : categoryProductsQuery.OrderBy(cp => cp.Order)).Skip(skip).Take(take).ToList().Select(cp => cp.Product));
                        viewModel.State.PageCount = (Int32)Math.Ceiling((Double)categoryProductsQuery.Count() / (Double)take);
                    }
                    else if (viewModel.State.SortExpression.StartsWith("price", StringComparison.InvariantCultureIgnoreCase))
                    {
                        IQueryable<ReferenceProduct> productsQuery = c.CatalogClient.Products
                                                                                        .Expand(p => p.Brand)
                                                                                        .Expand(p => p.Catalog)
                                                                                        .Expand(String.Format("{0}/VariableProduct/DefaultVariantProduct/Attributes/Files", c.CatalogClient.ResolveName(typeof(VariantProduct))))
                                                                                        .Expand(p => (p as VariantProduct).VariableProduct.DefaultVariantProduct.PriceWithLowerQuantity)
                                                                                        .Expand(p => (p as VariantProduct).VariableProduct.DefaultVariantProduct.Brand)
                                                                                        .Expand(p => (p as ReferenceProduct).PriceWithLowerQuantity)
                                                                                        .Expand(String.Format("{0}/Attributes/Files", c.CatalogClient.ResolveName(typeof(ReferenceProduct))))
                                                                                        .OfType<ReferenceProduct>()
                                                                                        .Where(p => !(p is VariantProduct) || (p is VariantProduct && (p as VariantProduct).VariableProduct.DefaultVariantProductId == p.ProductId));
                        if (!String.IsNullOrEmpty(viewModel.State.CatalogCodeFilter))
                        {
                            productsQuery = productsQuery.Where(p => p.Catalog.Code == viewModel.State.CatalogCodeFilter);
                        }
                        if (!String.IsNullOrEmpty(viewModel.State.CategoryCodeFilter))
                        {
                            productsQuery = productsQuery.Where(p => (!(p is VariantProduct) && p.ProductCategories.Any(pc => pc.Category.Code == viewModel.State.CategoryCodeFilter)) || (p is VariantProduct && (p as VariantProduct).VariableProduct.ProductCategories.Any(pc => pc.Category.Code == viewModel.State.CategoryCodeFilter)));
                        }
                        Expression<Func<ReferenceProduct, Object>> sort = this.GetMemberAccesExpression<ReferenceProduct>(viewModel.State.SortExpression);
                        viewModel.AddRange((viewModel.State.SortDirection == SortDirection.Descending ? productsQuery.OrderByDescending(sort) : productsQuery.OrderBy(sort)).Skip(skip).Take(take).ToList().Select(p => p is VariantProduct ? (p as VariantProduct).VariableProduct as BaseProduct : p));
                        viewModel.State.PageCount = (Int32)Math.Ceiling((Double)productsQuery.Count() / (Double)take);
                    }
                    else
                    {
                        IQueryable<BaseProduct> productsQuery = c.CatalogClient.Products
                                                                                    .Expand(String.Format("{0}/DefaultVariantProduct/Attributes/Files", c.CatalogClient.ResolveName(typeof(VariableProduct))))
                                                                                    .Expand(p => (p as VariableProduct).DefaultVariantProduct.PriceWithLowerQuantity)
                                                                                    .Expand(p => (p as VariableProduct).DefaultVariantProduct.Brand)
                                                                                    .Expand(p => (p as ReferenceProduct).PriceWithLowerQuantity)
                                                                                    .Expand(p => p.Brand)
                                                                                    .Expand(p => p.Catalog)
                                                                                    .Expand(String.Format("{0}/Attributes/Files", c.CatalogClient.ResolveName(typeof(ReferenceProduct))))
                                                                                    .Where(p => !(p is VariantProduct));
                        if (!String.IsNullOrEmpty(viewModel.State.CatalogCodeFilter))
                        {
                            productsQuery = productsQuery.Where(p => p.Catalog.Code == viewModel.State.CatalogCodeFilter);
                        }
                        if (!String.IsNullOrEmpty(viewModel.State.CategoryCodeFilter))
                        {
                            productsQuery = productsQuery.Where(p => (!(p is VariantProduct) && p.ProductCategories.Any(pc => pc.Category.Code == viewModel.State.CategoryCodeFilter)) || (p is VariantProduct && (p as VariantProduct).VariableProduct.ProductCategories.Any(pc => pc.Category.Code == viewModel.State.CategoryCodeFilter)));
                        }
                        Expression<Func<BaseProduct, Object>> sort = this.GetMemberAccesExpression<BaseProduct>(viewModel.State.SortExpression);
                        viewModel.AddRange((viewModel.State.SortDirection == SortDirection.Descending ? productsQuery.OrderByDescending(sort) : productsQuery.OrderBy(sort)).Skip(skip).Take(take).ToList().Select(p => p is VariantProduct ? (p as VariantProduct).VariableProduct as BaseProduct : p));
                        viewModel.State.PageCount = (Int32)Math.Ceiling((Double)productsQuery.Count() / (Double)take);
                    }
                }
            );
        }

        public ProductListServices(IWebStoreServices webStoreServices, IOrchardServices orchardServices, IUserModelsStateServices userModelsStateServices, ICatalogHierarchyServices catalogHierarchyServices)
        {
            this._webStoreServices = webStoreServices;
            this._userModelsStateServices = userModelsStateServices;
            this._catalogHierarchyServices = catalogHierarchyServices;
            this._orchardServices = orchardServices;
        }

        public ProductListViewModel GetModel(ProductListPart part)
        {
            ProductListViewModel viewModel = new ProductListViewModel();
            viewModel.State = this.GetState(part);
            this.UpdateState(part, viewModel);
            this.LoadProducts(part, viewModel);
            return viewModel;
        }
    }
}