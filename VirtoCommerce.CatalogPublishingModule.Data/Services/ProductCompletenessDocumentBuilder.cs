using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogPublishingModule.Core.Model.Search;
using VirtoCommerce.CatalogPublishingModule.Core.Services;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.SearchModule.Core.Model.Indexing;

namespace VirtoCommerce.CatalogPublishingModule.Data.Services
{
    public class ProductCompletenessDocumentBuilder : IBatchDocumentBuilder<CatalogProduct>
    {
        private readonly ICompletenessService _completenessService;
        private readonly ICompletenessEvaluator[] _completenessEvaluators;

        public ProductCompletenessDocumentBuilder(ICompletenessService completenessService, ICompletenessEvaluator[] completenessEvaluators)
        {
            _completenessService = completenessService;
            _completenessEvaluators = completenessEvaluators;
        }

        public void UpdateDocuments(IList<IDocument> documents, IList<CatalogProduct> items, object context)
        {
            var documentsByProductId = documents.Select((x, i) => new KeyValuePair<string, IDocument>(items[i].Id, x)).ToDictionary(x => x.Key, x => x.Value);
            var catalogIds = items.SelectMany(x => x.Outlines.Select(o => o.Items.FirstOrDefault()?.Id)).Distinct().ToArray();
            var productsByCatalogId = catalogIds.ToDictionary(x => x, x => items.Where(i => i.Outlines.Any(o => o.Items.FirstOrDefault()?.Id == x)));
            var channels =  _completenessService.SearchChannels(new CompletenessChannelSearchCriteria { CatalogIds = catalogIds, Take = int.MaxValue }).Results;
            if (!channels.IsNullOrEmpty())
            {
                foreach (var catalogId in catalogIds)
                {
                    foreach (var channel in channels.Where(x => x.CatalogId == catalogId))
                    {
                        var evaluator = _completenessEvaluators.FirstOrDefault(x => x.GetType().Name == channel.EvaluatorType);
                        var completenessEntries = evaluator?.EvaluateCompleteness(channel, productsByCatalogId[catalogId].ToArray());
                        if (!completenessEntries.IsNullOrEmpty())
                        {
                            foreach (var completenessEntry in completenessEntries)
                            {
                                documentsByProductId[completenessEntry.ProductId]
                                    .Add(new DocumentField("completeness_" + channel.Name.ToLower(), completenessEntry.CompletenessPercent,
                                        new[] { IndexStore.Yes, IndexType.NotAnalyzed }));
                            }
                        }
                    }
                }
            }
        }
    }
}