using Magelia.WebStore.Models.Parts;
using Magelia.WebStore.Models.ViewModels.Product;
using Orchard;

namespace Magelia.WebStore.Contracts
{
    public interface IProductServices : IDependency
    {
        ProductViewModel GetModel(ProductPart part);
    }
}
