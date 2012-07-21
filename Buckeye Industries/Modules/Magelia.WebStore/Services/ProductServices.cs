using System;
using System.Data.Services.Client;
using System.Linq;
using System.Web;
using Magelia.WebStore.Client;
using Magelia.WebStore.Contracts;
using Magelia.WebStore.Models.Parts;
using Magelia.WebStore.Models.ViewModels.Product;

namespace Magelia.WebStore.Services
{
    public class ProductServices : IProductServices
    {
        private IWebStoreServices _webStoreServices;

        private ProductViewModel GetProductReference(ProductPart part)
        {
            ProductViewModel viewModel = new ProductViewModel();
            if (part.FromUrl && !String.IsNullOrEmpty(part.CatalogCodeUrlParameterKey) && !String.IsNullOrEmpty(part.SKUUrlParameterKey))
            {
                viewModel.RequestedCatalogCode = HttpContext.Current.Request.QueryString[part.CatalogCodeUrlParameterKey];
                viewModel.RequestedSKU = HttpContext.Current.Request.QueryString[part.SKUUrlParameterKey];
            }
            else if (!part.FromUrl)
            {
                viewModel.RequestedCatalogCode = part.CatalogCode;
                viewModel.RequestedSKU = part.SKU;
            }
            return viewModel;
        }

        public ProductServices(IWebStoreServices webStoreServices)
        {
            this._webStoreServices = webStoreServices;
        }

        public ProductViewModel GetModel(ProductPart part)
        {
            ProductViewModel viewModel = this.GetProductReference(part);
            if (!String.IsNullOrEmpty(viewModel.RequestedCatalogCode) && !String.IsNullOrEmpty(viewModel.RequestedSKU))
            {
                this._webStoreServices.UsingClient(
                    c =>
                    {
                        String variantProductName = c.CatalogClient.ResolveName(typeof(VariantProduct));
                        viewModel.ReferenceProduct = (c.CatalogClient.Products
                                                                    .OfType<ReferenceProduct>() as DataServiceQuery<ReferenceProduct>)
                                                                    .Expand(rp => rp.Brand)
                                                                    .Expand(String.Format("{0}/VariableProduct/VariantProducts/Attributes/Files", variantProductName))
                                                                    .Expand(String.Format("{0}/VariableProduct/VariantProducts/PriceWithLowerQuantity", variantProductName))
                                                                    .Expand("Prices/TaxDetails")
                                                                    .Expand("Prices/DiscountDetails")
                                                                    .Expand(rp => rp.PriceWithLowerQuantity)
                                                                    .Expand("Attributes/Files")
                                                                    .Where(rp => rp.Catalog.Code == viewModel.RequestedCatalogCode && rp.SKU == viewModel.RequestedSKU)
                                                                    .FirstOrDefault();
                    }
                );
            }
            return viewModel;
        }
    }
}