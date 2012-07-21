using System;
using Magelia.WebStore.Models.Parts;
using Magelia.WebStore.Models.ViewModels.ProductList;
using Orchard;

namespace Magelia.WebStore.Contracts
{
    public interface IProductListServices : IDependency
    {
        String TargetParameterKey { get; }
        String PageParameterKey { get; }
        String SortDirectionParameterKey { get; }
        String SortExpressionParameterKey { get; }
        ProductListViewModel GetModel(ProductListPart part);
    }
}