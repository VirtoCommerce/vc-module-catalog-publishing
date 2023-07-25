using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogPublishingModule.Core.Model;
using VirtoCommerce.CatalogPublishingModule.Core.Model.Search;
using VirtoCommerce.CatalogPublishingModule.Core.Services;
using VirtoCommerce.CatalogPublishingModule.Data.Search.Indexing;
using VirtoCommerce.CoreModule.Core.Outlines;
using VirtoCommerce.Platform.Core.ChangeLog;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.SearchModule.Core.Model;
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

            var emptyDocuments = await documentBuilder.GetDocumentsAsync(new[] { "UnknownProductId" });
            Assert.NotNull(emptyDocuments);
            Assert.Empty(emptyDocuments);

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
            var changesProvider = new ProductCompletenessChangesProvider(GetChangeLogSearchService(), GetCompletenessService());

            var changesCount = await changesProvider.GetTotalChangesCountAsync(_startIndexDateTime, _endIndexDateTime);
            Assert.Equal(4, changesCount);

            var changes = await changesProvider.GetChangesAsync(_startIndexDateTime, _endIndexDateTime, 0, changesCount);
            Assert.Collection(changes,
                c => Assert.True(c.DocumentId == "First" && c.ChangeDate == DateTime.Parse("5/11/2017 1:00 PM") && c.ChangeType == IndexDocumentChangeType.Modified),
                c => Assert.True(c.DocumentId == "Second" && c.ChangeDate == DateTime.Parse("5/11/2017 2:00 PM") && c.ChangeType == IndexDocumentChangeType.Modified),
                c => Assert.True(c.DocumentId == "Second" && c.ChangeDate == DateTime.Parse("5/11/2017 3:00 PM") && c.ChangeType == IndexDocumentChangeType.Modified),
                c => Assert.True(c.DocumentId == "Third" && c.ChangeDate == DateTime.Parse("5/11/2017 4:00 PM") && c.ChangeType == IndexDocumentChangeType.Modified));
        }

        private IChangeLogSearchService GetChangeLogSearchService()
        {
            var service = new Mock<IChangeLogSearchService>();
            service.Setup(x => x.SearchAsync(It.Is<ChangeLogSearchCriteria>(t => t.ObjectType == "CompletenessEntryEntity" && t.StartDate == _startIndexDateTime & t.EndDate == _endIndexDateTime)))
                .Returns<ChangeLogSearchCriteria>((criteria) => Task.FromResult(new ChangeLogSearchResult()
                {
                    TotalCount = 4,
                    Results = new List<OperationLog>(new[]
                        {
                            new OperationLog
                            {
                                Id = "First",
                                CreatedDate = DateTime.Parse("5/11/2017 1:00 PM"),
                                CreatedBy = "Test",
                                ObjectType = criteria.ObjectType,
                                ObjectId = "First",
                                OperationType = EntryState.Added
                            },
                            new OperationLog
                            {
                                Id = "Second",
                                CreatedDate = DateTime.Parse("5/11/2017 2:00 PM"),
                                CreatedBy = "Test",
                                ObjectType = criteria.ObjectType,
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
                                ObjectType = criteria.ObjectType,
                                ObjectId = "Second",
                                OperationType = EntryState.Modified
                            },
                            new OperationLog
                            {
                                Id = "Fourth",
                                CreatedDate = DateTime.Parse("5/11/2017 4:00 PM"),
                                CreatedBy = "Test",
                                ObjectType = criteria.ObjectType,
                                ObjectId = "Third",
                                OperationType = EntryState.Added
                            },
                        }
                    )
                })
                );
            return service.Object;
        }

        private IItemService GetItemService()
        {
            var service = new Mock<IItemService>();
            service.Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns<IList<string>, string, bool>((ids, rg, c) => Task.FromResult<IList<CatalogProduct>>(_products.Where(p => ids.Contains(p.Id)).ToArray()));
            return service.Object;
        }

        private ICompletenessService GetCompletenessService()
        {
            var service = new Mock<ICompletenessService>();
            service.Setup(x => x.SearchChannelsAsync(It.Is<CompletenessChannelSearchCriteria>(c => c.CatalogIds.Any(id => id == _firstCatalogId || id == _secondCatalogId))))
                .Returns<CompletenessChannelSearchCriteria>(x => Task.FromResult(new CompletenessChannelSearchResult { Results = x.CatalogIds.Select(GetChannelByCatalogId).ToArray() }));
            service.Setup(x => x.GetCompletenessEntriesByIdsAsync(It.Is<string[]>(ids => _products.Select(p => p.Id).Intersect(ids).Any())))
                .Returns<string[]>(ids => Task.FromResult(_products.Select(p => GetCompletenessEntry(p.CatalogId, p.Id)).ToArray()));
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
            service.Setup(x => x.EvaluateCompletenessAsync(
                It.Is<CompletenessChannel>(c => c.CatalogId == _firstCatalogId || c.CatalogId == _secondCatalogId),
                It.Is<CatalogProduct[]>(cp => _products.Select(p => p.Id).Intersect(cp.Select(p => p.Id)).Any())))
                .Returns<CompletenessChannel, CatalogProduct[]>((c, x) => Task.FromResult(x.Select(p => GetCompletenessEntry(c.Id, p.Id)).ToArray()));
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
