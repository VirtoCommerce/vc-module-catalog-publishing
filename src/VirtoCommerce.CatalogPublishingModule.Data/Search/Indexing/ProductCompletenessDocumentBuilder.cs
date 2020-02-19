using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogPublishingModule.Core.Model.Search;
using VirtoCommerce.CatalogPublishingModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
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
        private readonly ICompletenessEvaluator[] _completenessEvaluators;

        public ProductCompletenessDocumentBuilder(IItemService itemService, ICompletenessService completenessService, IEnumerable<ICompletenessEvaluator> completenessEvaluators)
        {
            _itemService = itemService;
            _completenessService = completenessService;
            _completenessEvaluators = completenessEvaluators.ToArray();
        }

        public async Task<IList<IndexDocument>> GetDocumentsAsync(IList<string> documentIds)
        {
            var products = await GetProducts(documentIds);
            var documentsByProductId = documentIds.Distinct().ToDictionary(id => id, id => new IndexDocument(id));
            var catalogIds = products.SelectMany(p => p.Outlines.Select(o => o.Items.FirstOrDefault()?.Id)).Distinct().ToArray();
            var productsByCatalogId = catalogIds.ToDictionary(id => id, id => products.Where(p => p.Outlines.Any(o => o.Items.FirstOrDefault()?.Id == id)));

            var channelsSearchResult = await _completenessService.SearchChannelsAsync(new CompletenessChannelSearchCriteria { CatalogIds = catalogIds, Take = int.MaxValue });
            var channels = channelsSearchResult.Results;
            if (!channels.IsNullOrEmpty())
            {
                foreach (var catalogId in catalogIds)
                {
                    foreach (var channel in channels.Where(c => c.CatalogId == catalogId))
                    {
                        var evaluator = _completenessEvaluators.FirstOrDefault(e => e.GetType().Name == channel.EvaluatorType);
                        var completenessEntries = await evaluator?.EvaluateCompletenessAsync(channel, productsByCatalogId[catalogId].ToArray());
                        if (completenessEntries?.Any() == true)
                        {
                            foreach (var completenessEntry in completenessEntries)
                            {
                                var document = documentsByProductId[completenessEntry.ProductId];
                                document.Add(new IndexDocumentField($"completeness_{channel.Name}".ToLower(), completenessEntry.CompletenessPercent) { IsRetrievable = true, IsFilterable = true });
                            }
                        }
                    }
                }
            }

            IList<IndexDocument> result = documentsByProductId.Values.Where(d => d.Fields.Any()).ToArray();
            return result;
        }


        protected async virtual Task<IList<CatalogProduct>> GetProducts(IList<string> productIds)
        {
            return await _itemService.GetByIdsAsync(productIds.ToArray(), ItemResponseGroup.Outlines.ToString());
        }
    }
}
