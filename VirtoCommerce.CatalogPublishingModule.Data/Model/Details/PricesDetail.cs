using System.Linq;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Pricing.Model;

namespace VirtoCommerce.CatalogPublishingModule.Data.Model.Details
{
    public class PricesDetail : DefaultReadinessDetail
    {
        public PricesDetail()
        {
            Name = "Prices";
        }

        public override void Evaluate(CatalogProduct product, string pricelistId, Price[] productPrices, string language)
        {
            productPrices = productPrices?.Where(p => p.PricelistId == pricelistId).ToArray();
            ReadinessPercent = productPrices != null && productPrices.Any(x => x.List > 0) ? 100 : 0;
        }
    }
}