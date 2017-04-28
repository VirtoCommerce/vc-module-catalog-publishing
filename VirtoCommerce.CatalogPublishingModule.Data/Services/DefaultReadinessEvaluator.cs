using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogPublishingModule.Core.Model;
using VirtoCommerce.CatalogPublishingModule.Core.Services;
using VirtoCommerce.CatalogPublishingModule.Data.Model.Details;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Domain.Pricing.Model.Search;
using VirtoCommerce.Domain.Pricing.Services;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogPublishingModule.Data.Services
{
    public class DefaultReadinessEvaluator : IReadinessEvaluator
    {
        private readonly Func<DefaultReadinessDetail[]> _detailFactory;
        private readonly IItemService _productService;
        private readonly IPricingSearchService _pricingSearchService;

        public DefaultReadinessEvaluator(Func<DefaultReadinessDetail[]> detailFactory, IItemService productService, IPricingSearchService pricingSearchService)
        {
            _detailFactory = detailFactory;
            _productService = productService;
            _pricingSearchService = pricingSearchService;
        }

        public ReadinessEntry[] EvaluateReadiness(ReadinessChannel channel, CatalogProduct[] products)
        {
            if (channel == null)
            {
                throw new ArgumentNullException(nameof(channel));
            }
            if (products.IsNullOrEmpty())
            {
                throw new ArgumentException("Products must be specified", nameof(products));
            }
            var prices = _pricingSearchService.SearchPrices(new PricesSearchCriteria
            {
                PriceListId = channel.PricelistId, ProductIds = products.Select(x => x.Id).ToArray(), Take = int.MaxValue
            }).Results;

            products = _productService.GetByIds(products.Select(x => x.Id).ToArray(),
                ItemResponseGroup.ItemProperties | ItemResponseGroup.ItemEditorialReviews | ItemResponseGroup.Seo | ItemResponseGroup.Outlines).ToArray();
            products = products.Where(x => x.Outlines.Any(o => o.Items.FirstOrDefault()?.Id == channel.CatalogId )).ToArray();

            var retVal = new List<ReadinessEntry>(products.Length);
            
            foreach (var product in products)
            {
                var entry = new ReadinessEntry
                {
                    ChannelId = channel.Id,
                    ProductId = product.Id,
                    Details = _detailFactory().Select(x =>
                    {
                        x.Evaluate(product, channel.PricelistId, prices?.Where(p => p.ProductId == product.Id).ToArray(), channel.Language);
                        return x;
                    }).ToArray()
                };
                entry.ReadinessPercent = (int) Math.Round(entry.Details.Average(x => x.ReadinessPercent));
                retVal.Add(entry);
            }

            return retVal.ToArray();
        }
    }
}