using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogPublishingModule.Core.Model;
using VirtoCommerce.CatalogPublishingModule.Core.Model.Search;
using VirtoCommerce.CatalogPublishingModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.SearchModule.Core.Extensions;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;

namespace VirtoCommerce.CatalogPublishingModule.Data.Search.Indexing
{
    /// <summary>
    /// Extend product indexation process and provides completeness_ChannelName field for indexed products
    /// </summary>
    public class ProductCompletenessDocumentBuilder : IIndexDocumentBuilder
    {
        private readonly IItemService _itemService;
        private readonly ICompletenessService _completenessService;
        private readonly Dictionary<string, ICompletenessEvaluator> _evaluatorsByType;

        public ProductCompletenessDocumentBuilder(
            IItemService itemService,
            ICompletenessService completenessService,
            IEnumerable<ICompletenessEvaluator> completenessEvaluators)
        {
            _itemService = itemService;
            _completenessService = completenessService;
            _evaluatorsByType = completenessEvaluators.ToDictionary(x => x.GetType().Name);
        }

        public virtual async Task<IList<IndexDocument>> GetDocumentsAsync(IList<string> documentIds)
        {
            var products = await GetProducts(documentIds);
            var documentsByProductId = documentIds.Distinct().ToDictionary(id => id, id => new IndexDocument(id));
            var catalogIds = products.SelectMany(p => p.Outlines.Select(o => o.Items.FirstOrDefault()?.Id)).Distinct().ToArray();
            var productsByCatalogId = catalogIds.ToDictionary(id => id, id => products.Where(p => p.Outlines.Any(o => o.Items.FirstOrDefault()?.Id == id)).ToArray());
            var channels = await GetChannels(catalogIds);

            foreach (var channel in channels)
            {
                var completenessEntries = await EvaluateCompleteness(channel, productsByCatalogId[channel.CatalogId]);

                foreach (var completenessEntry in completenessEntries)
                {
                    var document = documentsByProductId[completenessEntry.ProductId];
                    var fieldName = $"completeness_{channel.Name}".ToLower();
                    document.AddFilterableInteger(fieldName, completenessEntry.CompletenessPercent);
                }
            }

            var result = documentsByProductId.Values.Where(d => d.Fields.Any()).ToArray();
            return result;
        }


        protected virtual async Task<IList<CatalogProduct>> GetProducts(IList<string> productIds)
        {
            if (productIds.IsNullOrEmpty())
            {
                return Array.Empty<CatalogProduct>();
            }

            return await _itemService.GetAsync(productIds, ItemResponseGroup.Outlines.ToString());
        }

        protected virtual async Task<IList<CompletenessChannel>> GetChannels(string[] catalogIds)
        {
            if (catalogIds.IsNullOrEmpty())
            {
                return Array.Empty<CompletenessChannel>();
            }

            var searchCriteria = new CompletenessChannelSearchCriteria { CatalogIds = catalogIds, Take = int.MaxValue };
            var searchResult = await _completenessService.SearchChannelsAsync(searchCriteria);

            return searchResult.Results;
        }

        protected virtual async Task<CompletenessEntry[]> EvaluateCompleteness(CompletenessChannel channel, CatalogProduct[] products)
        {
            if (products.IsNullOrEmpty() || !_evaluatorsByType.TryGetValue(channel.EvaluatorType, out var evaluator))
            {
                return Array.Empty<CompletenessEntry>();
            }

            var result = await evaluator.EvaluateCompletenessAsync(channel, products);

            return result;
        }
    }
}
