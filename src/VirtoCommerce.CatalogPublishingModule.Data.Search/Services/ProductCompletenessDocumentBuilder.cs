using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogPublishingModule.Core.Model.Search;
using VirtoCommerce.CatalogPublishingModule.Core.Services;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Domain.Search;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogPublishingModule.Data.Search.Services
{
    /// <summary>
    /// Extend product indexation process and provides completeness_ChannelName field for indexed products
    /// </summary>
    public class ProductCompletenessDocumentBuilder : IIndexDocumentBuilder
    {
        private readonly IItemService _itemService;
        private readonly ICompletenessService _completenessService;
        private readonly ICompletenessEvaluator[] _completenessEvaluators;

        public ProductCompletenessDocumentBuilder(IItemService itemService, ICompletenessService completenessService, ICompletenessEvaluator[] completenessEvaluators)
        {
            _itemService = itemService;
            _completenessService = completenessService;
            _completenessEvaluators = completenessEvaluators;
        }

        public Task<IList<IndexDocument>> GetDocumentsAsync(IList<string> documentIds)
        {
            var products = GetProducts(documentIds);
            var documentsByProductId = documentIds.Distinct().ToDictionary(id => id, id => new IndexDocument(id));
            var catalogIds = products.SelectMany(p => p.Outlines.Select(o => o.Items.FirstOrDefault()?.Id)).Distinct().ToArray();
            var productsByCatalogId = catalogIds.ToDictionary(id => id, id => products.Where(p => p.Outlines.Any(o => o.Items.FirstOrDefault()?.Id == id)));

            var channels = _completenessService.SearchChannels(new CompletenessChannelSearchCriteria { CatalogIds = catalogIds, Take = int.MaxValue }).Results;
            if (!channels.IsNullOrEmpty())
            {
                foreach (var catalogId in catalogIds)
                {
                    foreach (var channel in channels.Where(c => c.CatalogId == catalogId))
                    {
                        var evaluator = _completenessEvaluators.FirstOrDefault(e => e.GetType().Name == channel.EvaluatorType);
                        var completenessEntries = evaluator?.EvaluateCompleteness(channel, productsByCatalogId[catalogId].ToArray());
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
            return Task.FromResult(result);
        }


        protected virtual IList<CatalogProduct> GetProducts(IList<string> productIds)
        {
            return _itemService.GetByIds(productIds.ToArray(), ItemResponseGroup.Outlines);
        }
    }
}
