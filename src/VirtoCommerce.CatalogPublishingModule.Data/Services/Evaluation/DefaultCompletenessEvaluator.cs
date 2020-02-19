using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogPublishingModule.Core.Model;
using VirtoCommerce.CatalogPublishingModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogPublishingModule.Data.Services.Evaluation
{
    /// <summary>
    /// Default completeness evaluator. Provides completeness for properties, descriptions, prices &amp; seo by default
    /// </summary>
    public class DefaultCompletenessEvaluator : ICompletenessEvaluator
    {
        private readonly IItemService _productService;

        protected IReadOnlyCollection<ICompletenessDetailEvaluator> DetailEvaluators { get; }

        public DefaultCompletenessEvaluator(ICompletenessDetailEvaluator[] detailEvaluators, IItemService productService)
        {
            _productService = productService;
            DetailEvaluators = detailEvaluators;
        }

        public virtual async Task<CompletenessEntry[]> EvaluateCompletenessAsync(CompletenessChannel channel, CatalogProduct[] products)
        {
            ValidateParameters(channel, products);

            products = (await _productService.GetByIdsAsync(products.Select(x => x.Id).ToArray(), ItemResponseGroup.ItemLarge.ToString())).ToArray();
            products = products.Where(x => x.Outlines.Any(o => o.Items.FirstOrDefault()?.Id == channel.CatalogId)).ToArray();

            var details = new List<CompletenessDetail>(products.Length * DetailEvaluators.Count);
            foreach (var detailEvaluator in DetailEvaluators)
            {
                details.AddRange(await detailEvaluator.EvaluateCompletenessAsync(channel, products));
            }
            var entries = details.GroupBy(x => x.ProductId)
                .Select(x =>
                {
                    var entry = new CompletenessEntry { ChannelId = channel.Id, ProductId = x.Key, Details = x.AsEnumerable().ToArray() };
                    entry.CompletenessPercent = (int)Math.Floor(entry.Details.Average(d => d.CompletenessPercent));
                    return entry;
                });

            return entries.ToArray();
        }

        private static void ValidateParameters(CompletenessChannel channel, CatalogProduct[] products)
        {
            if (channel == null)
            {
                throw new ArgumentNullException(nameof(channel));
            }
            if (products.IsNullOrEmpty())
            {
                throw new ArgumentException("Products must be specified", nameof(products));
            }
        }
    }
}
