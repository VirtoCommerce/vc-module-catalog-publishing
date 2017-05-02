using System.Linq;
using VirtoCommerce.CatalogPublishingModule.Core.Model;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Pricing.Model.Search;
using VirtoCommerce.Domain.Pricing.Services;

namespace VirtoCommerce.CatalogPublishingModule.Data.Services.Evaluators
{
    public class PricesReadinessDetailEvaluator : DefaultReadinessDetailEvaluator
    {
        private readonly IPricingSearchService _pricingSearchService;

        public PricesReadinessDetailEvaluator(IPricingSearchService pricingSearchService)
        {
            _pricingSearchService = pricingSearchService;
        }

        public override ReadinessDetail[] EvaluateReadiness(ReadinessChannel channel, CatalogProduct[] products)
        {
            var prices = _pricingSearchService.SearchPrices(new PricesSearchCriteria
            {
                PriceListId = channel.PricelistId,
                ProductIds = products.Select(x => x.Id).ToArray(),
                Take = int.MaxValue
            }).Results;
            return products.Select(x =>
            {
                var detail = new ReadinessDetail { Name = "Prices", ProductId = x.Id };
                var productPrices = prices?.Where(p => p.ProductId == x.Id).ToArray();
                detail.ReadinessPercent = productPrices != null && productPrices.Any(p => p.List > 0) ? 100 : 0;
                return detail;
            }).ToArray();
        }
    }
}