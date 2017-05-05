﻿using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogPublishingModule.Core.Model.Search;
using VirtoCommerce.CatalogPublishingModule.Core.Services;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.SearchModule.Core.Model.Indexing;

namespace VirtoCommerce.CatalogPublishingModule.Data.Services
{
    public class ProductReadinessDocumentBuilder : IBatchDocumentBuilder<CatalogProduct>
    {
        private readonly IReadinessService _readinessService;
        private readonly IReadinessEvaluator[] _readinessEvaluators;

        public ProductReadinessDocumentBuilder(IReadinessService readinessService, IReadinessEvaluator[] readinessEvaluators)
        {
            _readinessService = readinessService;
            _readinessEvaluators = readinessEvaluators;
        }

        public void UpdateDocuments(IList<IDocument> documents, IList<CatalogProduct> items, object context)
        {
            var documentsByProductId = documents.Select((x, i) => new KeyValuePair<string, IDocument>(items[i].Id, x)).ToDictionary(x => x.Key, x => x.Value);
            var catalogIds = items.SelectMany(x => x.Outlines.Select(o => o.Items.FirstOrDefault()?.Id)).Distinct().ToArray();
            var productsByCatalogId = catalogIds.ToDictionary(x => x, x => items.Where(i => i.Outlines.Any(o => o.Items.FirstOrDefault()?.Id == x)));
            var channels =  _readinessService.SearchChannels(new ReadinessChannelSearchCriteria { CatalogIds = catalogIds, Take = int.MaxValue }).Results;
            if (!channels.IsNullOrEmpty())
            {
                foreach (var catalogId in catalogIds)
                {
                    foreach (var channel in channels.Where(x => x.CatalogId == catalogId))
                    {
                        var evaluator = _readinessEvaluators.FirstOrDefault(x => x.GetType().Name == channel.EvaluatorType);
                        var readinessEntries = evaluator?.EvaluateReadiness(channel, productsByCatalogId[catalogId].ToArray());
                        if (!readinessEntries.IsNullOrEmpty())
                        {
                            foreach (var readinessEntry in readinessEntries)
                            {
                                documentsByProductId[readinessEntry.ProductId]
                                    .Add(new DocumentField("readiness_" + channel.Name.ToLower(), readinessEntry.ReadinessPercent,
                                        new[] { IndexStore.Yes, IndexType.NotAnalyzed }));
                            }
                        }
                    }
                }
            }
        }
    }
}