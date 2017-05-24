using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogPublishingModule.Core.Services;
using VirtoCommerce.CatalogPublishingModule.Data.Model;
using VirtoCommerce.Platform.Core.ChangeLog;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.SearchModule.Core.Model.Indexing;

namespace VirtoCommerce.CatalogPublishingModule.Data.Search.Services
{
    /// <summary>
    /// Extend product indexation process. Invalidate products as changed when products completeness value updated.
    /// </summary>
    public class ProductCompletenessOperationProvider : IOperationProvider
    {
        private readonly IChangeLogService _changeLogService;
        private readonly ICompletenessService _completenessService;

        // TODO: Use CatalogItemSearchCriteria.DocType instead, when Catalog module will be updated. See VirtoCommerce.SearchApiModule.Data.Model.CatalogItemSearchCriteria
        public string DocumentType => "catalogitem";

        public ProductCompletenessOperationProvider(IChangeLogService changeLogService, ICompletenessService completenessService)
        {
            _changeLogService = changeLogService;
            _completenessService = completenessService;
        }

        public IList<Operation> GetOperations(DateTime startDate, DateTime endDate)
        {
            var allOperations = _changeLogService.FindChangeHistory(typeof(CompletenessEntryEntity).Name, startDate, endDate).ToArray();
            var completenessEntryIds = allOperations.Select(c => c.ObjectId).ToArray();
            var productIds = GetProductIds(completenessEntryIds);

            // TODO: Possible reusable code, and -WithPagingAndParallelism method too
            // TODO: How to get product for deleted completeness entry? See VirtoCommerce.SearchApiModule.Data.Services.ProductPriceOperationProvider
            var result = allOperations
                .Where(o => productIds.ContainsKey(o.ObjectId))
                .Select(o => new Operation { ObjectId = productIds[o.ObjectId], Timestamp = o.ModifiedDate ?? o.CreatedDate, OperationType = OperationType.Index })
                .GroupBy(o => o.ObjectId)
                .Select(g => g.OrderByDescending(o => o.Timestamp).First())
                .ToList();

            return result;
        }

        protected virtual IDictionary<string, string> GetProductIds(ICollection<string> completenessEntryIds)
        {
            // TODO: Get pageSize and degreeOfParallelism from settings. See VirtoCommerce.SearchApiModule.Data.Services.ProductPriceOperationProvider 
            return GetProductIdsWithPagingAndParallelism(completenessEntryIds, 1000, 10);
        }

        protected virtual IDictionary<string, string> GetProductIdsWithPagingAndParallelism(ICollection<string> completenessEntryIds, int pageSize, int degreeOfParallelism)
        {
            IDictionary<string, string> result;

            if (degreeOfParallelism > 1)
            {
                var dictionary = new ConcurrentDictionary<string, string>();

                var pages = new List<string[]>();
                completenessEntryIds.ProcessWithPaging(pageSize, (ids, skipCount, totalCount) => pages.Add(ids.ToArray()));

                var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = degreeOfParallelism };

                Parallel.ForEach(pages, parallelOptions, ids =>
                {
                    var completenessEntries = _completenessService.GetCompletenessEntriesByIds(ids);
                    foreach (var completenessEntry in completenessEntries)
                    {
                        var productId = completenessEntry.ProductId;
                        dictionary.AddOrUpdate(completenessEntry.Id, productId, (key, oldValue) => productId);
                    }
                });

                result = dictionary;
            }
            else
            {
                var dictionary = new Dictionary<string, string>();

                completenessEntryIds.ProcessWithPaging(pageSize, (ids, skipCount, totalCount) =>
                {
                    foreach (var completenessEntry in _completenessService.GetCompletenessEntriesByIds(ids.ToArray()))
                    {
                        dictionary[completenessEntry.Id] = completenessEntry.ProductId;
                    }
                });

                result = dictionary;
            }

            return result;
        }
    }
}