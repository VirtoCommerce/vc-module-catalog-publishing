using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogPublishingModule.Core.Services;
using VirtoCommerce.CatalogPublishingModule.Data.Model;
using VirtoCommerce.Platform.Core.ChangeLog;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;

namespace VirtoCommerce.CatalogPublishingModule.Data.Search.Indexing
{
    /// <summary>
    /// Extend product indexation process. Invalidate products as changed when products completeness value updated.
    /// </summary>
    public class ProductCompletenessChangesProvider : IIndexDocumentChangesProvider
    {
        public const string ChangeLogObjectType = nameof(CompletenessEntryEntity);

        private readonly IChangeLogSearchService _changeLogSearchService;
        private readonly ICompletenessService _completenessService;

        public ProductCompletenessChangesProvider(IChangeLogSearchService changeLogSearchService, ICompletenessService completenessService)
        {
            _changeLogSearchService = changeLogSearchService;
            _completenessService = completenessService;
        }

        public async Task<long> GetTotalChangesCountAsync(DateTime? startDate, DateTime? endDate)
        {
            long result;

            if (startDate == null && endDate == null)
            {
                // We don't know the total products count
                result = 0L;
            }
            else
            {
                // Get changes count from operation log
                result = (await _changeLogSearchService.SearchAsync(
                    new ChangeLogSearchCriteria()
                    {
                        ObjectType = ChangeLogObjectType,
                        StartDate = startDate,
                        EndDate = endDate
                    })
                ).TotalCount;
            }

            return result;
        }

        public async Task<IList<IndexDocumentChange>> GetChangesAsync(DateTime? startDate, DateTime? endDate, long skip, long take)
        {
            IList<IndexDocumentChange> result;

            if (startDate == null && endDate == null)
            {
                result = null;
            }
            else
            {
                var operations = (await _changeLogSearchService.SearchAsync(
                     new ChangeLogSearchCriteria()
                     {
                         ObjectType = ChangeLogObjectType,
                         StartDate = startDate,
                         EndDate = endDate,
                         Skip = (int)skip,
                         Take = (int)take,
                     }))
                .Results;

                var completenessEntryIds = operations.Select(o => o.ObjectId).ToArray();
                var entryIdsAndProductIds = await GetProductIdsAsync(completenessEntryIds);

                result = operations
                    .Where(o => entryIdsAndProductIds.ContainsKey(o.ObjectId))
                    .Select(o => new IndexDocumentChange
                    {
                        DocumentId = entryIdsAndProductIds[o.ObjectId],
                        ChangeDate = o.ModifiedDate ?? o.CreatedDate,
                        ChangeType = IndexDocumentChangeType.Modified,
                    })
                    .ToArray();
            }

            return result;
        }

        protected async virtual Task<IDictionary<string, string>> GetProductIdsAsync(string[] completenessEntryIds)
        {
            // TODO: How to get product for deleted completeness entry?
            var completenessEntries = await _completenessService.GetCompletenessEntriesByIdsAsync(completenessEntryIds);
            var result = completenessEntries.ToDictionary(e => e.Id, e => e.ProductId);
            return result;
        }
    }
}
