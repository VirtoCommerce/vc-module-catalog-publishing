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
            var productsByCatalogId = items.GroupBy(x => x.CatalogId);
            foreach (var productsPerCatalog in productsByCatalogId)
            {
                var channelsSearch = _readinessService.SearchChannels(new ReadinessChannelSearchCriteria { CatalogId = productsPerCatalog.Key, Take = int.MaxValue });
                if (!channelsSearch.Results.IsNullOrEmpty())
                {
                    foreach (var channel in channelsSearch.Results)
                    {
                        var evaluator = _readinessEvaluators.FirstOrDefault(x => x.GetType().Name == channel.EvaluatorType);
                        var readinessEntries = evaluator?.EvaluateReadiness(channel, productsPerCatalog.Select(x => x).ToArray());
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