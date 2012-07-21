using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using Magelia.WebStore.Client;
using Magelia.WebStore.Contracts;
using Magelia.WebStore.Extensions;
using Magelia.WebStore.Models.Parts;
using Magelia.WebStore.Models.ViewModels.CatalogHierarchy;

namespace Magelia.WebStore.Services
{
    public class CatalogHierarchyServices : ICatalogHierarchyServices
    {
        private class LoadCategoriesInstruction
        {
            public Guid CatalogId { get; private set; }
            public String Parent { get; private set; }
            public Action<IEnumerable<CategoryItemViewModel>> OnLoad { get; private set; }

            public LoadCategoriesInstruction(Guid catalogId, String parent, Action<IEnumerable<CategoryItemViewModel>> onLoad)
            {
                this.CatalogId = catalogId;
                this.Parent = parent;
                this.OnLoad = onLoad;
            }
        }

        private class HierarchyPath
        {
            public String Catalog { get; private set; }

            public List<String> Categories { get; private set; }

            public Boolean TargetCatalog
            {
                get
                {
                    return !String.IsNullOrEmpty(this.Catalog) && !this.Categories.Any();
                }
            }

            public HierarchyPath(String catalog, IEnumerable<String> categories)
            {
                this.Catalog = catalog;
                this.Categories = categories.ToList();
            }

            public HierarchyPath(Char pathSeparator, String path)
            {
                this.Categories = new List<String>();
                if (!String.IsNullOrEmpty(path))
                {
                    String[] fragments = path.Split(pathSeparator);
                    for (Int32 i = 0; i < fragments.Length; i++)
                    {
                        String fragment = fragments.ElementAt(i);
                        if (i == 0)
                        {
                            this.Catalog = fragment;
                        }
                        else
                        {
                            this.Categories.Add(fragment);
                        }
                    }
                }
            }
        }

        private const String HierarchyRequestKeyPrefix = "hierarchy-";
        private const String HierarchyUserModelsStateCategory = "hierarchy";
        private const String RootCategoryCode = "root";

        private IUserModelsStateServices _userModelsStateServices;
        private IWebStoreServices _webStoreServices;

        private String _action
        {
            get
            {
                return HttpContext.Current.Request.Url.GetAddedParameter(this.ActionParameterKey);
            }
        }

        private String _path
        {
            get
            {
                return HttpContext.Current.Request.Url.GetAddedParameter(this.PathParameterKey);
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

        public Char PathSeparator
        {
            get
            {
                return '|';
            }
        }

        public String TargetParameterKey
        {
            get
            {
                return "chx_target";
            }
        }

        public String ActionParameterKey
        {
            get
            {
                return "chw_action";
            }
        }

        public String PathParameterKey
        {
            get
            {
                return "chw_path";
            }
        }

        public String SelectParameterValue
        {
            get
            {
                return "select";
            }
        }

        public String ExpandParameterValue
        {
            get
            {
                return "expand";
            }
        }

        public String CollapseParameterValue
        {
            get
            {
                return "collapse";
            }
        }

        private HierarchyViewModel EnsureHierarchy(CatalogHierarchyPart part)
        {
            return this._userModelsStateServices.GetFromCommerceContext<HierarchyViewModel>(CatalogHierarchyServices.HierarchyUserModelsStateCategory, part.Id); ;
        }

        private IEnumerable<CatalogItemViewModel> RefreshCatalogs(HierarchyViewModel hierarchy)
        {
            List<CatalogItemViewModel> previouslyLoadedCatalogs = hierarchy.ToList();
            this._webStoreServices.UsingClient(
                c =>
                {
                    hierarchy.Clear();
                    hierarchy.AddRange(
                        c.CatalogClient.Catalogs
                                            .Expand(cata => cata.Files)
                                            .ToList()
                                            .OrderBy(cata => cata.Name)
                                            .Select(
                                                cata =>
                                                {
                                                    CatalogItemViewModel catalog = previouslyLoadedCatalogs.FirstOrDefault(pcata => pcata.Catalog.CatalogId == cata.CatalogId) ?? new CatalogItemViewModel();
                                                    catalog.Catalog = cata;
                                                    return catalog;
                                                }
                                            )
                    );
                }
            );
            return hierarchy.Where(cata => !previouslyLoadedCatalogs.Any(pcata => pcata.Catalog.CatalogId == cata.Catalog.CatalogId));
        }

        private Action<IEnumerable<CategoryItemViewModel>> CreateAppender(HierarchyItemViewModel item)
        {
            return this.CreateAppender(() => item);
        }

        private Action<IEnumerable<CategoryItemViewModel>> CreateAppender(HierarchyViewModel hierarchy, HierarchyPath hierarchyPath)
        {
            return this.CreateAppender(() => this.FindItem(hierarchy, hierarchyPath));
        }

        private Action<IEnumerable<CategoryItemViewModel>> CreateAppender(Func<HierarchyItemViewModel> itemGetter)
        {
            return categories =>
            {
                HierarchyItemViewModel item = itemGetter();
                if (item != null)
                {
                    List<CategoryItemViewModel> previousCategories = item.Categories;
                    item.Categories = categories.ToList();
                    item.Categories.ForEach(
                        cate =>
                        {
                            CategoryItemViewModel previousCategory = previousCategories.FirstOrDefault(pcate => pcate.Category.CategoryId == cate.Category.CategoryId);
                            if (previousCategory != null)
                            {
                                cate.Expanded = previousCategory.Expanded;
                                cate.Selected = previousCategory.Selected;
                                cate.Categories = previousCategory.Categories;
                            }
                        }
                    );
                    item.Expanded = true;
                }
            };
        }

        private void Initialize(HierarchyViewModel hierarchy, HierarchyPath hierarchyPath, List<LoadCategoriesInstruction> loadCategoriesInstructions)
        {
            IEnumerable<CatalogItemViewModel> newlyLoadedCatalogs = this.RefreshCatalogs(hierarchy).Where(cata => !cata.Catalog.Code.EqualsInvariantCultureIgnoreCase(hierarchyPath.Catalog));
            loadCategoriesInstructions.AddRange(
                (hierarchy.Count == 1 ? hierarchy : newlyLoadedCatalogs).Select(
                        cata => new LoadCategoriesInstruction(
                            cata.Catalog.CatalogId,
                            CatalogHierarchyServices.RootCategoryCode,
                                this.CreateAppender(cata)
                        )
                    )
            );
        }

        private HierarchyItemViewModel FindItem(HierarchyViewModel hierarchy, HierarchyPath hierarchyPath)
        {
            HierarchyItemViewModel item = hierarchy.FirstOrDefault(cata => cata.Catalog.Code.EqualsInvariantCultureIgnoreCase(hierarchyPath.Catalog));
            if (item != null)
            {
                foreach (String categoryCode in hierarchyPath.Categories)
                {
                    item = item.Categories.FirstOrDefault(cate => cate.Category.Code.EqualsInvariantCultureIgnoreCase(categoryCode));
                    if (item == null)
                    {
                        break;
                    }
                }
            }
            return item;
        }

        private void EnsurePath(HierarchyViewModel hierarchy, HierarchyPath hierarchyPath, List<LoadCategoriesInstruction> loadCategoriesInstructions)
        {
            CatalogItemViewModel catalog = hierarchy.FirstOrDefault(cata => cata.Catalog.Code.EqualsInvariantCultureIgnoreCase(hierarchyPath.Catalog));
            if (catalog != null)
            {
                loadCategoriesInstructions.AddRange(
                    hierarchyPath.Categories
                                    .Select((c, i) => new { Code = c, Index = i })
                                    .Where(o => this.FindItem(hierarchy, new HierarchyPath(catalog.Catalog.Code, hierarchyPath.Categories.Take(o.Index + 1))) == null)
                                    .Select(
                                         o => new LoadCategoriesInstruction(
                                            catalog.Catalog.CatalogId,
                                            o.Index == 0 ? CatalogHierarchyServices.RootCategoryCode : hierarchyPath.Categories.ElementAtOrDefault(o.Index - 1),
                                            this.CreateAppender(hierarchy, new HierarchyPath(catalog.Catalog.Code, hierarchyPath.Categories.Take(o.Index)))
                                        )
                                    )
                );
            }
        }

        private void Expand(HierarchyViewModel hierarchy, HierarchyPath hierarchyPath, List<LoadCategoriesInstruction> loadCategoriesInstructions)
        {
            CatalogItemViewModel catalog = hierarchy.FirstOrDefault(cata => cata.Catalog.Code.EqualsInvariantCultureIgnoreCase(hierarchyPath.Catalog));
            if (catalog != null)
            {
                loadCategoriesInstructions.Add(
                    new LoadCategoriesInstruction(
                        catalog.Catalog.CatalogId,
                        hierarchyPath.TargetCatalog ? CatalogHierarchyServices.RootCategoryCode : hierarchyPath.Categories.Last(),
                        this.CreateAppender(hierarchy, hierarchyPath)
                    )
                );
            }
        }

        private void Collapse(HierarchyViewModel hierarchy, HierarchyPath hierarchyPath)
        {
            HierarchyItemViewModel item = this.FindItem(hierarchy, hierarchyPath);
            if (item != null)
            {
                item.Categories.Clear();
                item.Expanded = false;
            }
        }

        private void Execute(HierarchyViewModel hierarchy, List<LoadCategoriesInstruction> loadCategoriesInstructions)
        {
            if (loadCategoriesInstructions.Any())
            {
                this._webStoreServices.UsingClient(
                    c =>
                    {
                        List<CategoryHierarchy> categoryhierarchies = c.CatalogClient.CategoryHierarchies.Expand(ch => ch.ParentCategory).Expand(ch => ch.Category.Files).Expand(ch => ch.Category.Catalog).Where(this.GetConditions(loadCategoriesInstructions)).ToList();
                        loadCategoriesInstructions.ForEach(lci => lci.OnLoad(categoryhierarchies.Where(ch => ch.ParentCategory.Code.EqualsInvariantCultureIgnoreCase(lci.Parent) && ch.Category.CatalogId == lci.CatalogId).OrderBy(ch => ch.Order).Select(ch => new CategoryItemViewModel { Category = ch.Category })));
                    }
                );
            }
        }

        private String GetPropertyName<T, P>(Expression<Func<T, P>> accessor)
        {
            return (accessor.Body as MemberExpression).Member.Name;
        }

        private Expression<Func<CategoryHierarchy, Boolean>> GetConditions(List<LoadCategoriesInstruction> loadCategoriesInstructions)
        {
            Expression conditions = null;
            ParameterExpression categoryHierarchyParameter = Expression.Parameter(typeof(CategoryHierarchy));
            loadCategoriesInstructions.ForEach(
                lci =>
                {
                    Expression condition = Expression.And(
                        Expression.And(
                            Expression.Equal(
                                Expression.Property(
                                    Expression.Property(
                                        categoryHierarchyParameter,
                                        this.GetPropertyName<CategoryHierarchy, Category>(ch => ch.ParentCategory)
                                    ),
                                    this.GetPropertyName<Category, String>(cate => cate.Code)
                                ),
                                Expression.Constant(lci.Parent)
                            ),
                            Expression.Equal(
                                Expression.Property(
                                    Expression.Property(
                                        categoryHierarchyParameter,
                                        this.GetPropertyName<CategoryHierarchy, Category>(ch => ch.Category)
                                    ),
                                    this.GetPropertyName<Category, Guid>(ch => ch.CatalogId)
                                ),
                                Expression.Constant(lci.CatalogId)
                            )
                        ),
                        Expression.Property(
                            Expression.Property(
                                categoryHierarchyParameter,
                                this.GetPropertyName<CategoryHierarchy, Category>(ch => ch.Category)
                            ),
                            this.GetPropertyName<Category, Boolean>(ch => ch.IsOnline)
                        )
                    );
                    conditions = conditions == null ? condition : Expression.Or(conditions, condition);
                }
            );
            return Expression.Lambda<Func<CategoryHierarchy, Boolean>>(conditions, categoryHierarchyParameter);
        }

        private void UnselectAll(IEnumerable<HierarchyItemViewModel> items)
        {
            foreach (HierarchyItemViewModel item in items)
            {
                item.Selected = false;
                this.UnselectAll(item.Categories);
            }
        }

        private void Select(HierarchyViewModel hierarchy, HierarchyPath hierarchyPath)
        {
            this.UnselectAll(hierarchy);
            HierarchyItemViewModel item = this.FindItem(hierarchy, hierarchyPath);
            if (item != null)
            {
                item.Selected = true;
            }
        }

        private Boolean HasSelection(IEnumerable<HierarchyItemViewModel> items)
        {
            foreach (HierarchyItemViewModel item in items)
            {
                if (item.Selected || this.HasSelection(item.Categories))
                {
                    return true;
                }
            }
            return false;
        }

        private void EnsureSelection(CatalogHierarchyPart part, HierarchyViewModel hierarchy)
        {
            IEnumerable<HierarchyItemViewModel> selectables = hierarchy.Count == 1 ? hierarchy.SelectMany(cata => cata.Categories) : hierarchy.Cast<HierarchyItemViewModel>();
            if (part.GenerateUrls)
            {
                this.UnselectAll(hierarchy);
            }
            else if (!this.HasSelection(selectables))
            {
                this.UnselectAll(hierarchy);
                HierarchyItemViewModel item = selectables.FirstOrDefault();
                if (item != null)
                {
                    item.Selected = true;
                }
            }
        }

        private String GetHierarchyRequestKey(CatalogHierarchyPart part)
        {
            return String.Concat(CatalogHierarchyServices.HierarchyRequestKeyPrefix, part.Id);
        }

        private HierarchyViewModel GetRequestHieararchy(CatalogHierarchyPart part)
        {
            return HttpContext.Current.Items[this.GetHierarchyRequestKey(part)] as HierarchyViewModel;
        }

        private void RegisterHierarchyForRequest(CatalogHierarchyPart part, HierarchyViewModel hierarchy)
        {
            HttpContext.Current.Items[this.GetHierarchyRequestKey(part)] = hierarchy;
        }

        public CatalogHierarchyServices(IWebStoreServices webStoreServices, IUserModelsStateServices userModelsStateServices)
        {
            this._webStoreServices = webStoreServices;
            this._userModelsStateServices = userModelsStateServices;
        }

        public HierarchyViewModel GetModel(CatalogHierarchyPart part)
        {
            HierarchyViewModel hierarchy = this.GetRequestHieararchy(part);
            if (hierarchy == null)
            {
                Boolean isTarget = this._target == part.Id;
                hierarchy = this.EnsureHierarchy(part);
                HierarchyPath hierarchyPath = new HierarchyPath(this.PathSeparator, isTarget ? this._path : null);
                List<LoadCategoriesInstruction> loadCategoriesInstructions = new List<LoadCategoriesInstruction>();
                this.Initialize(hierarchy, hierarchyPath, loadCategoriesInstructions);
                if (isTarget)
                {
                    this.EnsurePath(hierarchy, hierarchyPath, loadCategoriesInstructions);
                    if (this.ExpandParameterValue.EqualsInvariantCultureIgnoreCase(this._action))
                    {
                        this.Expand(hierarchy, hierarchyPath, loadCategoriesInstructions);
                    }
                    this.Execute(hierarchy, loadCategoriesInstructions);
                    if (this.CollapseParameterValue.EqualsInvariantCultureIgnoreCase(this._action))
                    {
                        this.Collapse(hierarchy, hierarchyPath);
                    }
                    if (this.SelectParameterValue.EqualsInvariantCultureIgnoreCase(this._action))
                    {
                        this.Select(hierarchy, hierarchyPath);
                    }
                }
                else
                {
                    this.Execute(hierarchy, loadCategoriesInstructions);
                }
                this.EnsureSelection(part, hierarchy);
                this.RegisterHierarchyForRequest(part, hierarchy);
            }
            return hierarchy;
        }
    }
}