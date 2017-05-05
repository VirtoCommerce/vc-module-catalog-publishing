using System.Collections.Generic;
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
    [Trait("Category", "CI")]
    public class IndexationTests
    {
        private const string FirstCatalogId = "Test1";
        private const string SecondCatalogId = "Test2";

        private CatalogProduct[] _products =
        {
            new CatalogProduct
            {
                Id = "First",
                Outlines = new List<Outline> { new Outline { Items = new List<OutlineItem> { new OutlineItem { Id = FirstCatalogId } } } }
            },
            new CatalogProduct
            {
                Id = "Second",
                Outlines = new List<Outline> { new Outline { Items = new List<OutlineItem> { new OutlineItem { Id = FirstCatalogId } } } }
            },
            new CatalogProduct
            {
                Id = "Third",
                Outlines = new List<Outline> { new Outline { Items = new List<OutlineItem> { new OutlineItem { Id = SecondCatalogId } } } }
            },
            new CatalogProduct
            {
                Id = "Fourth",
                Outlines = new List<Outline> { new Outline { Items = new List<OutlineItem> { new OutlineItem { Id = SecondCatalogId } } } }
            }
        };

        [Fact]
        public void Test()
        {
            var documentBuilder = new ProductReadinessDocumentBuilder(GetReadinessService(), new []{ GetReadinessEvaluator() });
            var documents = _products.Select(x => new ResultDocument() as IDocument).ToArray();
            documentBuilder.UpdateDocuments(documents, _products, null);
            for (var i = 0; i < documents.Length; i++)
            {
                var document = documents[i];
                Assert.True(document.FieldCount == 1 && (int) document["readiness_" + _products[i].Outlines.FirstOrDefault()?.Items.FirstOrDefault()?.Id.ToLower()].Value == 50);
            }
        }

        private IReadinessService GetReadinessService()
        {
            var service = new Mock<IReadinessService>();
            service.Setup(x => x.SearchChannels(It.Is<ReadinessChannelSearchCriteria>(c => c.CatalogIds.Any(id => id == FirstCatalogId || id == SecondCatalogId))))
                .Returns<ReadinessChannelSearchCriteria>(x => new GenericSearchResult<ReadinessChannel> { Results = x.CatalogIds.Select(GetChannelByCatalogId).ToArray() });
            return service.Object;
        }

        private ReadinessChannel GetChannelByCatalogId(string catalogId)
        {
            return new ReadinessChannel
            {
                Name = catalogId,
                Languages = new List<string> { "Test1", "Test2" },
                Currencies = new List<string> { "Test1", "Test2" },
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
                new ReadinessDetail
                {
                    Name= "Test1",
                    ReadinessPercent = 25
                },
                new ReadinessDetail
                {
                    Name= "Test2",
                    ReadinessPercent = 25
                },
                new ReadinessDetail
                {
                    Name= "Test3",
                    ReadinessPercent = 25
                },
                new ReadinessDetail
                {
                    Name= "Test4",
                    ReadinessPercent = 25
                }
            };
        }
    }
}