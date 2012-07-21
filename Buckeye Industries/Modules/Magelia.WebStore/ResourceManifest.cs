using Orchard.UI.Resources;

namespace Magelia.WebStore
{
    public class ResourceManifest : IResourceManifestProvider
    {
        public void BuildManifests(ResourceManifestBuilder builder)
        {
            var manifest = builder.Add();
            manifest.DefineStyle("Magelia").SetUrl("Magelia.css");
            manifest.DefineScript("Magelia").SetUrl("~/Magelia.WebStore/Context/Magelia").SetDependencies("jQuery");
            manifest.DefineScript("Magelia.Address").SetUrl("Libraries/Magelia/Magelia.Address.js").SetDependencies("Magelia");
            manifest.DefineScript("Magelia.AddressesManager").SetUrl("Libraries/Magelia/Magelia.AddressesManager.js").SetDependencies("Magelia.Address");
            manifest.DefineScript("Magelia.LocationPicker").SetUrl("Libraries/Magelia/Magelia.LocationPicker.js").SetDependencies("Magelia");
            manifest.DefineScript("Magelia.CurrencyPicker").SetUrl("Libraries/Magelia/Magelia.CurrencyPicker.js").SetDependencies("Magelia");
            manifest.DefineScript("Magelia.AddToBasket").SetUrl("Libraries/Magelia/Magelia.AddToBasket.js").SetDependencies("Magelia");
            manifest.DefineScript("Magelia.VariantPicker").SetUrl("Libraries/Magelia/Magelia.VariantPicker.js").SetDependencies("Magelia");
            manifest.DefineScript("Magelia.BasketCount").SetUrl("Libraries/Magelia/Magelia.BasketCount.js").SetDependencies("Magelia");
            manifest.DefineScript("Magelia.Basket").SetUrl("Libraries/Magelia/Magelia.Basket.js").SetDependencies("Magelia");
            manifest.DefineScript("Magelia.Checkout").SetUrl("Libraries/Magelia/Magelia.Checkout.js").SetDependencies("Magelia");
            manifest.DefineScript("Magelia.ShippingRates").SetUrl("Libraries/Magelia/Magelia.ShippingRates.js").SetDependencies("Magelia");
            manifest.DefineScript("Magelia.Payment").SetUrl("Libraries/Magelia/Magelia.Payment.js").SetDependencies("Magelia");
            manifest.DefineScript("Magelia.Order").SetUrl("Libraries/Magelia/Magelia.Order.js").SetDependencies("Magelia");
            manifest.DefineScript("Magelia.Orders").SetUrl("Libraries/Magelia/Magelia.Orders.js").SetDependencies("Magelia.Order");
        }
    }
}