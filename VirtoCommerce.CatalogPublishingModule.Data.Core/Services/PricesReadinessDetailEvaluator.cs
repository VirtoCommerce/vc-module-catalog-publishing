using System.Linq;
using VirtoCommerce.CatalogPublishingModule.Core.Model;
using VirtoCommerce.CatalogPublishingModule.Data.Core.Common;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Pricing.Model.Search;
using VirtoCommerce.Domain.Pricing.Services;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogPublishingModule.Data.Core.Services
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
                ProductIds = products.Select(x => x.Id).ToArray(),
                Take = int.MaxValue
            }).Results;
            return products.Select(x =>
            {
                var detail = new ReadinessDetail { Name = "Prices", ProductId = x.Id };
                var currenciesWithoutValidPrice = channel.Currencies.Where(c =>
                {
                    var productPricesForCurrency = prices?.Where(p => p.ProductId == x.Id && p.Currency == c).ToArray();
                    return productPricesForCurrency.IsNullOrEmpty() || productPricesForCurrency.All(p => p.List <= 0);
                });
                detail.ReadinessPercent = ReadinessHelper.CalculateReadiness(channel.Currencies.Count, currenciesWithoutValidPrice.Count());
                return detail;
            }).ToArray();
        }
    }
}