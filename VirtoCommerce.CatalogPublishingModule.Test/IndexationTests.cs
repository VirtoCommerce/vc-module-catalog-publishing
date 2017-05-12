using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using VirtoCommerce.CatalogPublishingModule.Core.Model;
using VirtoCommerce.CatalogPublishingModule.Core.Model.Search;
using VirtoCommerce.CatalogPublishingModule.Core.Services;
using VirtoCommerce.CatalogPublishingModule.Data.Services;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Commerce.Model.Search;
using VirtoCommerce.Platform.Core.ChangeLog;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.SearchModule.Core.Model.Indexing;
using Xunit;

namespace VirtoCommerce.CatalogPublishingModule.Test
{
    [Trait("Category", "CI")]
    public class IndexationTests
    {
        private const string FirstCatalogId = "Test1";
        private const string SecondCatalogId = "Test2";
        private readonly DateTime _startIndexDateTime = DateTime.Parse("5/11/2017 12:00 PM");
        private readonly DateTime _endIndexDateTime = DateTime.Parse("5/12/2017 12:00 AM");

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
        public void TestDocumentBuilder()
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

        [Fact]
        public void TestOperationProvider()
        {
            var operationProvider = new ProductReadinessOperationProvider(GetChangeLogService(), GetReadinessService());
            Assert.True(operationProvider.DocumentType == "catalogitem");
            var operations = operationProvider.GetOperations(_startIndexDateTime, _endIndexDateTime);
            Assert.Collection(operations, o => Assert.True(o.ObjectId == "First" && o.Timestamp == DateTime.Parse("5/11/2017 1:00 PM") && o.OperationType == OperationType.Index),
                o => Assert.True(o.ObjectId == "Second" && o.Timestamp == DateTime.Parse("5/11/2017 3:00 PM") && o.OperationType == OperationType.Index),
                o => Assert.True(o.ObjectId == "Third" && o.Timestamp == DateTime.Parse("5/11/2017 4:00 PM") && o.OperationType == OperationType.Index));
        }

        private IChangeLogService GetChangeLogService()
        {
            var service = new Mock<IChangeLogService>();
            service.Setup(x => x.FindChangeHistory(It.Is<string>(t => t == "ReadinessEntryEntity"),
                    It.Is<DateTime>(d => d == _startIndexDateTime),
                    It.Is<DateTime>(d => d == _endIndexDateTime)))
                .Returns<string, DateTime, DateTime>((t, sd, ed) => new[]
                {
                    new OperationLog
                    {
                        Id = "First",
                        CreatedDate = DateTime.Parse("5/11/2017 1:00 PM"),
                        CreatedBy = "Test",
                        ObjectType = t,
                        ObjectId = "First",
                        OperationType = EntryState.Added
                    },
                    new OperationLog
                    {
                        Id = "Second",
                        CreatedDate = DateTime.Parse("5/11/2017 2:00 PM"),
                        CreatedBy = "Test",
                        ObjectType = t,
                        ObjectId = "Second",
                        OperationType = EntryState.Added
                    },
                    new OperationLog
                    {
                        Id = "Third",
                        CreatedDate = DateTime.Parse("5/11/2017 2:00 PM"),
                        CreatedBy = "Test",
                        ModifiedDate = DateTime.Parse("5/11/2017 3:00 PM"),
                        ModifiedBy = "Test",
                        ObjectType = t,
                        ObjectId = "Second",
                        OperationType = EntryState.Modified
                    },
                    new OperationLog
                    {
                        Id = "Fourth",
                        CreatedDate = DateTime.Parse("5/11/2017 4:00 PM"),
                        CreatedBy = "Test",
                        ObjectType = t,
                        ObjectId = "Third",
                        OperationType = EntryState.Added
                    }
                });
            return service.Object;
        }

        private IReadinessService GetReadinessService()
        {
            var service = new Mock<IReadinessService>();
            service.Setup(x => x.SearchChannels(It.Is<ReadinessChannelSearchCriteria>(c => c.CatalogIds.Any(id => id == FirstCatalogId || id == SecondCatalogId))))
                .Returns<ReadinessChannelSearchCriteria>(x => new GenericSearchResult<ReadinessChannel> { Results = x.CatalogIds.Select(GetChannelByCatalogId).ToArray() });
            service.Setup(x => x.GetReadinessEntriesByIds(It.Is<string[]>(ids => _products.Select(p => p.Id).Intersect(ids).Any())))
                .Returns<string[]>(ids => _products.Select(p => GetReadinessEntry(p.CatalogId, p.Id)).ToArray());
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
                .Returns<ReadinessChannel, CatalogProduct[]>((c, x) => x.Select(p => GetReadinessEntry(c.Id, p.Id)).ToArray());
            return service.Object;
        }

        private static ReadinessEntry GetReadinessEntry(string channelId, string productId)
        {
            return new ReadinessEntry
            {
                Id = productId,
                ChannelId = channelId,
                ProductId = productId,
                ReadinessPercent = 50,
                Details = GetDetails()
            };
        }

        private static ReadinessDetail[] GetDetails()
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