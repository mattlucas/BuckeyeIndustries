using System;
using Magelia.WebStore.Models.Parts;
using Magelia.WebStore.Models.ViewModels.CatalogHierarchy;
using Orchard;

namespace Magelia.WebStore.Contracts
{
    public interface ICatalogHierarchyServices : IDependency
    {
        Char PathSeparator { get; }
        String TargetParameterKey { get; }
        String ActionParameterKey { get; }
        String PathParameterKey { get; }
        String SelectParameterValue { get; }
        String ExpandParameterValue { get; }
        String CollapseParameterValue { get; }
        HierarchyViewModel GetModel(CatalogHierarchyPart part);
    }
}
