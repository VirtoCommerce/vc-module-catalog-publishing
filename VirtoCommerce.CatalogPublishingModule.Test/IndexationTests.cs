using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using VirtoCommerce.CatalogPublishingModule.Core.Model;
using VirtoCommerce.CatalogPublishingModule.Core.Model.Search;
using VirtoCommerce.CatalogPublishingModule.Core.Services;
using VirtoCommerce.CatalogPublishingModule.Data.Search.Services;
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
            var documentBuilder = new ProductCompletenessDocumentBuilder(GetCompletenessService(), new []{ GetCompletenessEvaluator() });
            var documents = _products.Select(x => new ResultDocument() as IDocument).ToArray();
            documentBuilder.UpdateDocuments(documents, _products, null);
            for (var i = 0; i < documents.Length; i++)
            {
                var document = documents[i];
                Assert.True(document.FieldCount == 1 && (int) document["completeness_" + _products[i].Outlines.FirstOrDefault()?.Items.FirstOrDefault()?.Id.ToLower()].Value == 50);
            }
        }

        [Fact]
        public void TestOperationProvider()
        {
            var operationProvider = new ProductCompletenessOperationProvider(GetChangeLogService(), GetCompletenessService());
            Assert.True(operationProvider.DocumentType == "catalogitem");
            var operations = operationProvider.GetOperations(_startIndexDateTime, _endIndexDateTime);
            Assert.Collection(operations, o => Assert.True(o.ObjectId == "First" && o.Timestamp == DateTime.Parse("5/11/2017 1:00 PM") && o.OperationType == OperationType.Index),
                o => Assert.True(o.ObjectId == "Second" && o.Timestamp == DateTime.Parse("5/11/2017 3:00 PM") && o.OperationType == OperationType.Index),
                o => Assert.True(o.ObjectId == "Third" && o.Timestamp == DateTime.Parse("5/11/2017 4:00 PM") && o.OperationType == OperationType.Index));
        }

        private IChangeLogService GetChangeLogService()
        {
            var service = new Mock<IChangeLogService>();
            service.Setup(x => x.FindChangeHistory(It.Is<string>(t => t == "CompletenessEntryEntity"),
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

        private ICompletenessService GetCompletenessService()
        {
            var service = new Mock<ICompletenessService>();
            service.Setup(x => x.SearchChannels(It.Is<CompletenessChannelSearchCriteria>(c => c.CatalogIds.Any(id => id == FirstCatalogId || id == SecondCatalogId))))
                .Returns<CompletenessChannelSearchCriteria>(x => new GenericSearchResult<CompletenessChannel> { Results = x.CatalogIds.Select(GetChannelByCatalogId).ToArray() });
            service.Setup(x => x.GetCompletenessEntriesByIds(It.Is<string[]>(ids => _products.Select(p => p.Id).Intersect(ids).Any())))
                .Returns<string[]>(ids => _products.Select(p => GetCompletenessEntry(p.CatalogId, p.Id)).ToArray());
            return service.Object;
        }

        private CompletenessChannel GetChannelByCatalogId(string catalogId)
        {
            return new CompletenessChannel
            {
                Name = catalogId,
                Languages = new List<string> { "Test1", "Test2" },
                Currencies = new List<string> { "Test1", "Test2" },
                CatalogId = catalogId,
                EvaluatorType = GetCompletenessEvaluator().GetType().Name,
                CompletenessPercent = 0
            };
        }

        private ICompletenessEvaluator GetCompletenessEvaluator()
        {
            var service = new Mock<ICompletenessEvaluator>();
            service.Setup(x => x.EvaluateCompleteness(
                It.Is<CompletenessChannel>(c => c.CatalogId == FirstCatalogId || c.CatalogId == SecondCatalogId),
                It.Is<CatalogProduct[]>(cp => _products.Select(p => p.Id).Intersect(cp.Select(p => p.Id)).Any())))
                .Returns<CompletenessChannel, CatalogProduct[]>((c, x) => x.Select(p => GetCompletenessEntry(c.Id, p.Id)).ToArray());
            return service.Object;
        }

        private static CompletenessEntry GetCompletenessEntry(string channelId, string productId)
        {
            return new CompletenessEntry
            {
                Id = productId,
                ChannelId = channelId,
                ProductId = productId,
                CompletenessPercent = 50,
                Details = GetDetails()
            };
        }

        private static CompletenessDetail[] GetDetails()
        {
            return new[]
            {
                new CompletenessDetail
                {
                    Name= "Test1",
                    CompletenessPercent = 25
                },
                new CompletenessDetail
                {
                    Name= "Test2",
                    CompletenessPercent = 25
                },
                new CompletenessDetail
                {
                    Name= "Test3",
                    CompletenessPercent = 25
                },
                new CompletenessDetail
                {
                    Name= "Test4",
                    CompletenessPercent = 25
                }
            };
        }
    }
}