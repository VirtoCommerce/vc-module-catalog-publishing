using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogPublishingModule.Core.Model;
using VirtoCommerce.CatalogPublishingModule.Core.Services;
using VirtoCommerce.Platform.Core.ChangeLog;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.SearchModule.Core.Model.Indexing;

namespace VirtoCommerce.CatalogPublishingModule.Data.Services
{
    public class ProductReadinessOperationProvider : IOperationProvider
    {
        private readonly IChangeLogService _changeLogService;
        private readonly IReadinessService _readinessService;

        // TODO: Use CatalogItemSearchCriteria.DocType instead, when Catalog module will be updated. See VirtoCommerce.SearchApiModule.Data.Model.CatalogItemSearchCriteria
        public string DocumentType => "catalogitem";

        public ProductReadinessOperationProvider(IChangeLogService changeLogService, IReadinessService readinessService)
        {
            _changeLogService = changeLogService;
            _readinessService = readinessService;
        }

        public IList<Operation> GetOperations(DateTime startDate, DateTime endDate)
        {
            var allReadinessEntryChanges = _changeLogService.FindChangeHistory("ReadinessEntry", startDate, endDate).ToArray();
            var readinessEntryIds = allReadinessEntryChanges.Select(c => c.ObjectId).Distinct().ToArray();
            var readinessEntries = GetReadinessEntries(readinessEntryIds);

            // TODO: Possible reusable code, and -WithPagingAndParallelism method too
            var result = allReadinessEntryChanges
                .Select(c => new { Timestamp = c.ModifiedDate ?? c.CreatedDate, ReadinessEntry = readinessEntries.ContainsKey(c.ObjectId) ? readinessEntries[c.ObjectId] : null })
                .Where(x => x.ReadinessEntry != null)
                .Select(x => new Operation { ObjectId = x.ReadinessEntry.ProductId, Timestamp = x.Timestamp, OperationType = OperationType.Index })
                .GroupBy(o => o.ObjectId)
                .Select(g => g.OrderByDescending(o => o.Timestamp).First())
                .ToList();

            return result;
        }

        protected virtual IDictionary<string, ReadinessEntry> GetReadinessEntries(ICollection<string> readinessEntryIds)
        {
            // TODO: Get pageSize and degreeOfParallelism from settings. See VirtoCommerce.SearchApiModule.Data.Services.ProductPriceOperationProvider 
            return GetReadinessEntriesWithPagingAndParallelism(readinessEntryIds, 1000, 10);
        }

        protected virtual IDictionary<string, ReadinessEntry> GetReadinessEntriesWithPagingAndParallelism(ICollection<string> readinessEntryIds, int pageSize, int degreeOfParallelism)
        {
            IDictionary<string, ReadinessEntry> result;

            if (degreeOfParallelism > 1)
            {
                var dictionary = new ConcurrentDictionary<string, ReadinessEntry>();

                var pages = new List<string[]>();
                readinessEntryIds.ProcessWithPaging(pageSize, (ids, skipCount, totalCount) => pages.Add(ids.ToArray()));

                var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = degreeOfParallelism };

                Parallel.ForEach(pages, parallelOptions, ids =>
                {
                    var readinessEntries = _readinessService.GetReadinessEntriesByIds(ids);
                    foreach (var readinessEntry in readinessEntries)
                    {
                        dictionary.AddOrUpdate(readinessEntry.Id, readinessEntry, (key, oldValue) => readinessEntry);
                    }
                });

                result = dictionary;
            }
            else
            {
                var dictionary = new Dictionary<string, ReadinessEntry>();

                readinessEntryIds.ProcessWithPaging(pageSize, (ids, skipCount, totalCount) =>
                {
                    foreach (var readinessEntry in _readinessService.GetReadinessEntriesByIds(ids.ToArray()))
                    {
                        dictionary[readinessEntry.Id] = readinessEntry;
                    }
                });

                result = dictionary;
            }

            return result;
        }
    }
}