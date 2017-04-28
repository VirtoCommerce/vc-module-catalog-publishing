using VirtoCommerce.CatalogPublishingModule.Core.Model;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Pricing.Model;

namespace VirtoCommerce.CatalogPublishingModule.Data.Model.Details
{
    public abstract class DefaultReadinessDetail : ReadinessDetail
    {
        public abstract void Evaluate(CatalogProduct product, string pricelistId, Price[] productPrices, string language);
    }
}