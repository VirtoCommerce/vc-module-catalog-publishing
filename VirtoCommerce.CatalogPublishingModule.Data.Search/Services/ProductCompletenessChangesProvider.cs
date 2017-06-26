using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogPublishingModule.Core.Services;
using VirtoCommerce.CatalogPublishingModule.Data.Model;
using VirtoCommerce.Domain.Search;
using VirtoCommerce.Platform.Core.ChangeLog;

namespace VirtoCommerce.CatalogPublishingModule.Data.Search.Services
{
    /// <summary>
    /// Extend product indexation process. Invalidate products as changed when products completeness value updated.
    /// </summary>
    public class ProductCompletenessChangesProvider : IIndexDocumentChangesProvider
    {
        public const string ChangeLogObjectType = nameof(CompletenessEntryEntity);

        private readonly IChangeLogService _changeLogService;
        private readonly ICompletenessService _completenessService;

        public ProductCompletenessChangesProvider(IChangeLogService changeLogService, ICompletenessService completenessService)
        {
            _changeLogService = changeLogService;
            _completenessService = completenessService;
        }

        public Task<long> GetTotalChangesCountAsync(DateTime? startDate, DateTime? endDate)
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
                result = _changeLogService.FindChangeHistory(ChangeLogObjectType, startDate, endDate).Count();
            }

            return Task.FromResult(result);
        }

        public Task<IList<IndexDocumentChange>> GetChangesAsync(DateTime? startDate, DateTime? endDate, long skip, long take)
        {
            IList<IndexDocumentChange> result;

            if (startDate == null && endDate == null)
            {
                result = null;
            }
            else
            {
                var operations = _changeLogService.FindChangeHistory(ChangeLogObjectType, startDate, endDate)
                    .Skip((int)skip)
                    .Take((int)take)
                    .ToArray();

                var completenessEntryIds = operations.Select(o => o.ObjectId).ToArray();
                var entryIdsAndProductIds = GetProductIds(completenessEntryIds);

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

            return Task.FromResult(result);
        }

        protected virtual IDictionary<string, string> GetProductIds(string[] completenessEntryIds)
        {
            // TODO: How to get product for deleted completeness entry?
            var completenessEntries = _completenessService.GetCompletenessEntriesByIds(completenessEntryIds);
            var result = completenessEntries.ToDictionary(e => e.Id, e => e.ProductId);
            return result;
        }
    }
}
