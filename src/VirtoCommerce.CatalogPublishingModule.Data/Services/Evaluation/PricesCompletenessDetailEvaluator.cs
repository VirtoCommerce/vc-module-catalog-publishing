using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogPublishingModule.Core.Model;
using VirtoCommerce.CatalogPublishingModule.Core.Services;
using VirtoCommerce.CatalogPublishingModule.Data.Common;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.PricingModule.Core.Model.Search;
using VirtoCommerce.PricingModule.Core.Services;

namespace VirtoCommerce.CatalogPublishingModule.Data.Services.Evaluation
{
    /// <summary>
    ///  Prices validator. Check that products have at least one price per channel currency, which list price is greater than zero.
    /// </summary>
    public class PricesCompletenessDetailEvaluator : ICompletenessDetailEvaluator
    {
        private const int _pageSize = 100;
        private readonly IPriceSearchService _pricingSearchService;

        public PricesCompletenessDetailEvaluator(IPriceSearchService pricingSearchService)
        {
            _pricingSearchService = pricingSearchService;
        }

        public async Task<CompletenessDetail[]> EvaluateCompletenessAsync(CompletenessChannel channel, CatalogProduct[] products)
        {
            var prices = (await _pricingSearchService.SearchAllNoCloneAsync(new PricesSearchCriteria
            {
                ProductIds = products.Select(x => x.Id).ToArray(),
                Take = _pageSize,
            }));

            return products.Select(x =>
            {
                var detail = new CompletenessDetail { Name = "Prices", ProductId = x.Id };
                var currenciesWithoutValidPrice = channel.Currencies.Where(c =>
                {
                    var productPricesForCurrency = prices?.Where(p => p.ProductId == x.Id && p.Currency == c).ToArray();
                    return productPricesForCurrency.IsNullOrEmpty() || productPricesForCurrency.All(p => p.List <= 0);
                });
                detail.CompletenessPercent = CompletenessHelper.CalculateCompleteness(channel.Currencies.Count, currenciesWithoutValidPrice.Count());
                return detail;
            }).ToArray();
        }
    }
}
