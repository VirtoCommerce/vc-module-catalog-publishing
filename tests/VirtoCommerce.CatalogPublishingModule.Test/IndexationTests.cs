using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using VirtoCommerce.CatalogPublishingModule.Core.Model;
using VirtoCommerce.CatalogPublishingModule.Core.Model.Search;
using VirtoCommerce.CatalogPublishingModule.Core.Services;
using VirtoCommerce.CatalogPublishingModule.Data.Search.Services;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Domain.Commerce.Model.Search;
using VirtoCommerce.Domain.Search;
using VirtoCommerce.Platform.Core.ChangeLog;
using VirtoCommerce.Platform.Core.Common;
using Xunit;

namespace VirtoCommerce.CatalogPublishingModule.Test
{
    [Trait("Category", "CI")]
    public class IndexationTests
    {
        private const string _firstCatalogId = "Test1";
        private const string _secondCatalogId = "Test2";
        private readonly DateTime _startIndexDateTime = DateTime.Parse("5/11/2017 12:00 PM");
        private readonly DateTime _endIndexDateTime = DateTime.Parse("5/12/2017 12:00 AM");

        private readonly CatalogProduct[] _products =
        {
            new CatalogProduct
            {
                Id = "First",
                Outlines = new List<Outline> { new Outline { Items = new List<OutlineItem> { new OutlineItem { Id = _firstCatalogId } } } }
            },
            new CatalogProduct
            {
                Id = "Second",
                Outlines = new List<Outline> { new Outline { Items = new List<OutlineItem> { new OutlineItem { Id = _firstCatalogId } } } }
            },
            new CatalogProduct
            {
                Id = "Third",
                Outlines = new List<Outline> { new Outline { Items = new List<OutlineItem> { new OutlineItem { Id = _secondCatalogId } } } }
            },
            new CatalogProduct
            {
                Id = "Fourth",
                Outlines = new List<Outline> { new Outline { Items = new List<OutlineItem> { new OutlineItem { Id = _secondCatalogId } } } }
            }
        };

        [Fact]
        public async Task TestDocumentBuilder()
        {
            var documentBuilder = new ProductCompletenessDocumentBuilder(GetItemService(), GetCompletenessService(), new[] { GetCompletenessEvaluator() });
            var productIds = _products.Select(p => p.Id).ToArray();

            var documents = await documentBuilder.GetDocumentsAsync(productIds);

            foreach (var product in _products)
            {
                var document = documents.FirstOrDefault(d => d.Id.EqualsInvariant(product.Id));
                Assert.NotNull(document);
                Assert.Equal(1, document.Fields.Count);

                var catalogId = product.Outlines.First().Items.First().Id;
                var fieldName = $"completeness_{catalogId}";
                var completenessField = document.Fields.FirstOrDefault(f => f.Name.EqualsInvariant(fieldName));
                Assert.NotNull(completenessField);
                Assert.Equal(50, completenessField.Value);
            }
        }

        [Fact]
        public async Task TestOperationProvider()
        {
            var changesProvider = new ProductCompletenessChangesProvider(GetChangeLogService(), GetCompletenessService());

            var changesCount = await changesProvider.GetTotalChangesCountAsync(_startIndexDateTime, _endIndexDateTime);
            Assert.Equal(4, changesCount);

            var changes = await changesProvider.GetChangesAsync(_startIndexDateTime, _endIndexDateTime, 0, changesCount);
            Assert.Collection(changes,
                c => Assert.True(c.DocumentId == "First" && c.ChangeDate == DateTime.Parse("5/11/2017 1:00 PM") && c.ChangeType == IndexDocumentChangeType.Modified),
                c => Assert.True(c.DocumentId == "Second" && c.ChangeDate == DateTime.Parse("5/11/2017 2:00 PM") && c.ChangeType == IndexDocumentChangeType.Modified),
                c => Assert.True(c.DocumentId == "Second" && c.ChangeDate == DateTime.Parse("5/11/2017 3:00 PM") && c.ChangeType == IndexDocumentChangeType.Modified),
                c => Assert.True(c.DocumentId == "Third" && c.ChangeDate == DateTime.Parse("5/11/2017 4:00 PM") && c.ChangeType == IndexDocumentChangeType.Modified));
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

        private IItemService GetItemService()
        {
            var service = new Mock<IItemService>();
            service.Setup(x => x.GetByIds(It.IsAny<string[]>(), It.IsAny<ItemResponseGroup>(), It.IsAny<string>()))
                .Returns<string[], ItemResponseGroup, string>((ids, rg, c) => _products.Where(p => ids.Contains(p.Id)).ToArray());
            return service.Object;
        }

        private ICompletenessService GetCompletenessService()
        {
            var service = new Mock<ICompletenessService>();
            service.Setup(x => x.SearchChannels(It.Is<CompletenessChannelSearchCriteria>(c => c.CatalogIds.Any(id => id == _firstCatalogId || id == _secondCatalogId))))
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
                It.Is<CompletenessChannel>(c => c.CatalogId == _firstCatalogId || c.CatalogId == _secondCatalogId),
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