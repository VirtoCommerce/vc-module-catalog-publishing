using System.Linq;
using Moq;
using VirtoCommerce.CatalogPublishingModule.Core.Model;
using VirtoCommerce.CatalogPublishingModule.Core.Model.Search;
using VirtoCommerce.CatalogPublishingModule.Core.Services;
using VirtoCommerce.CatalogPublishingModule.Data.Services;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Commerce.Model.Search;
using VirtoCommerce.SearchModule.Core.Model.Indexing;
using Xunit;

namespace VirtoCommerce.CatalogPublishingModule.Test
{
    public class IndexationTests
    {
        private const string FirstCatalogId = "Test1";
        private const string SecondCatalogId = "Test2";

        private CatalogProduct[] _products = {
            new CatalogProduct
            {
                Id = "First",
                CatalogId = FirstCatalogId
            },
            new CatalogProduct
            {
                Id = "Second",
                CatalogId = FirstCatalogId
            },
            new CatalogProduct
            {
                Id = "Third",
                CatalogId = SecondCatalogId
            },
            new CatalogProduct
            {
                Id = "Fourth",
                CatalogId = SecondCatalogId
            }
        };

        [Fact]
        public void Test()
        {
            var documentBuilder = new ProductReadinessDocumentBuilder(GetReadinessService(), new []{ GetReadinessEvaluator() });
            var documents = Enumerable.Repeat(new ResultDocument() as IDocument, _products.Length).ToList();
            documentBuilder.UpdateDocuments(documents, _products, null);
            for (var i = 0; i < documents.Count; i++)
            {
                var document = documents[i];
                Assert.True((int) document["readiness_" + _products[i].CatalogId].Value == 50);
            }
        }

        private IReadinessService GetReadinessService()
        {
            var service = new Mock<IReadinessService>();
            service.Setup(x => x.SearchChannels(It.Is<ReadinessChannelSearchCriteria>(c => c.CatalogId == FirstCatalogId || c.CatalogId == SecondCatalogId)))
                .Returns<ReadinessChannelSearchCriteria>(x => new GenericSearchResult<ReadinessChannel> { Results = new[] { GetChannelByCatalogId(x.CatalogId) }});
            return service.Object;
        }

        private ReadinessChannel GetChannelByCatalogId(string catalogId)
        {
            return new ReadinessChannel
            {
                Name = catalogId,
                Language = "Test",
                PricelistId = "Test",
                CatalogId = catalogId,
                EvaluatorType = GetReadinessEvaluator().GetType().Name,
                ReadinessPercent = 0
            };
        }

        private IReadinessEvaluator GetReadinessEvaluator()
        {
            var service = new Mock<IReadinessEvaluator>();
            service.Setup(x => x.EvaluateReadiness(
                It.Is<ReadinessChannel>(c => c.CatalogId == FirstCatalogId || c.CatalogId == SecondCatalogId),
                It.Is<CatalogProduct[]>(cp => _products.Select(p => p.Id).Intersect(cp.Select(p => p.Id)).Any())))
                .Returns<ReadinessChannel, CatalogProduct[]>((c, x) => x.Select(p => new ReadinessEntry
                    {
                        ChannelId = c.Id,
                        ProductId = p.Id,
                        ReadinessPercent = 50,
                        Details = GetDetails()
                    })
                    .ToArray());
            return service.Object;
        }

        private ReadinessDetail[] GetDetails()
        {
            return new[]
            {
                new ReadinessDetail { Name = "Properties", ReadinessPercent = 25 },
                new ReadinessDetail { Name = "Descriptions", ReadinessPercent = 25 },
                new ReadinessDetail { Name = "Prices", ReadinessPercent = 25 },
                new ReadinessDetail { Name = "Seo", ReadinessPercent = 25 },
            };
        }
    }
}