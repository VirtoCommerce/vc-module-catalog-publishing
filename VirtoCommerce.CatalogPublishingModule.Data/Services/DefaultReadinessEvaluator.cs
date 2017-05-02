using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogPublishingModule.Core.Model;
using VirtoCommerce.CatalogPublishingModule.Core.Services;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogPublishingModule.Data.Services
{
    public class DefaultReadinessEvaluator : IReadinessEvaluator
    {
        private readonly IReadinessDetailEvaluator[] _detailEvaluators;
        private readonly IItemService _productService;

        public DefaultReadinessEvaluator(DefaultReadinessDetailEvaluator[] detailEvaluators, IItemService productService) :
            this(detailEvaluators as IReadinessDetailEvaluator[], productService)
        {
        }

        protected DefaultReadinessEvaluator(IReadinessDetailEvaluator[] detailEvaluators, IItemService productService)
        {
            _detailEvaluators = detailEvaluators;
            _productService = productService;
        }

        public virtual ReadinessEntry[] EvaluateReadiness(ReadinessChannel channel, CatalogProduct[] products)
        {
            if (channel == null)
            {
                throw new ArgumentNullException(nameof(channel));
            }
            if (products.IsNullOrEmpty())
            {
                throw new ArgumentException("Products must be specified", nameof(products));
            }

            products = _productService.GetByIds(products.Select(x => x.Id).ToArray(), ItemResponseGroup.ItemLarge).ToArray();
            products = products.Where(x => x.Outlines.Any(o => o.Items.FirstOrDefault()?.Id == channel.CatalogId)).ToArray();

            var details = new List<ReadinessDetail>(products.Length * _detailEvaluators.Length);
            foreach (var detailEvaluator in _detailEvaluators)
            {
                details.AddRange(detailEvaluator.EvaluateReadiness(channel, products));
            }
            var entries = details.GroupBy(x => x.ProductId)
                .Select(x =>
                {
                    var entry = new ReadinessEntry { ChannelId = channel.Id, ProductId = x.Key, Details = x.AsEnumerable().ToArray() };
                    entry.ReadinessPercent = (int) Math.Round(entry.Details.Average(d => d.ReadinessPercent));
                    return entry;
                });

            return entries.ToArray();
        }
    }
}