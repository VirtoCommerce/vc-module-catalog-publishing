using System.Linq;
using VirtoCommerce.CatalogPublishingModule.Core.Model.Search;
using VirtoCommerce.CatalogPublishingModule.Core.Services;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.SearchApiModule.Data.Model;
using VirtoCommerce.SearchModule.Core.Model.Indexing;

namespace VirtoCommerce.CatalogPublishingModule.Data.Services
{
    public class ProductReadinessDocumentBuilder : IDocumentBuilder<CatalogProduct, ProductDocumentBuilderContext>
    {
        private readonly IReadinessService _readinessService;
        private readonly IReadinessEvaluator[] _readinessEvaluators;

        public ProductReadinessDocumentBuilder(IReadinessService readinessService, IReadinessEvaluator[] readinessEvaluators)
        {
            _readinessService = readinessService;
            _readinessEvaluators = readinessEvaluators;
        }

        public bool UpdateDocument(IDocument document, CatalogProduct item, ProductDocumentBuilderContext context)
        {
            var channelsSearch = _readinessService.SearchChannels(new ReadinessChannelSearchCriteria { CatalogId = item.CatalogId, Take = int.MaxValue });
            foreach (var channel in channelsSearch.Results)
            {
                var evaluator = _readinessEvaluators.FirstOrDefault(x => x.GetType().Name == channel.EvaluatorType);
                var readiness = evaluator?.EvaluateReadiness(channel, new[] { item }).FirstOrDefault()?.ReadinessPercent;
                if (readiness != null)
                {
                    document.Add(new DocumentField("readiness_" + channel.Name.ToLower(), readiness, new[] { IndexStore.Yes, IndexType.NotAnalyzed }));
                }
            }
            return true;
        }
    }
}
