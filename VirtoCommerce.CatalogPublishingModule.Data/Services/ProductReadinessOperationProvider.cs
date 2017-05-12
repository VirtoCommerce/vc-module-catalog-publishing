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
            var allOperations = _changeLogService.FindChangeHistory(typeof(ReadinessEntryEntity).Name, startDate, endDate).ToArray();
            var readinessEntryIds = allOperations.Select(c => c.ObjectId).ToArray();
            var productIds = GetProductIds(readinessEntryIds);

            // TODO: Possible reusable code, and -WithPagingAndParallelism method too
            // TODO: How to get product for deleted readiness entry? See VirtoCommerce.SearchApiModule.Data.Services.ProductPriceOperationProvider
            var result = allOperations
                .Where(o => productIds.ContainsKey(o.ObjectId))
                .Select(o => new Operation { ObjectId = productIds[o.ObjectId], Timestamp = o.ModifiedDate ?? o.CreatedDate, OperationType = OperationType.Index })
                .GroupBy(o => o.ObjectId)
                .Select(g => g.OrderByDescending(o => o.Timestamp).First())
                .ToList();

            return result;
        }

        protected virtual IDictionary<string, string> GetProductIds(ICollection<string> readinessEntryIds)
        {
            // TODO: Get pageSize and degreeOfParallelism from settings. See VirtoCommerce.SearchApiModule.Data.Services.ProductPriceOperationProvider 
            return GetProductIdsWithPagingAndParallelism(readinessEntryIds, 1000, 10);
        }

        protected virtual IDictionary<string, string> GetProductIdsWithPagingAndParallelism(ICollection<string> readinessEntryIds, int pageSize, int degreeOfParallelism)
        {
            IDictionary<string, string> result;

            if (degreeOfParallelism > 1)
            {
                var dictionary = new ConcurrentDictionary<string, string>();

                var pages = new List<string[]>();
                readinessEntryIds.ProcessWithPaging(pageSize, (ids, skipCount, totalCount) => pages.Add(ids.ToArray()));

                var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = degreeOfParallelism };

                Parallel.ForEach(pages, parallelOptions, ids =>
                {
                    var readinessEntries = _readinessService.GetReadinessEntriesByIds(ids);
                    foreach (var readinessEntry in readinessEntries)
                    {
                        var productId = readinessEntry.ProductId;
                        dictionary.AddOrUpdate(readinessEntry.Id, productId, (key, oldValue) => productId);
                    }
                });

                result = dictionary;
            }
            else
            {
                var dictionary = new Dictionary<string, string>();

                readinessEntryIds.ProcessWithPaging(pageSize, (ids, skipCount, totalCount) =>
                {
                    foreach (var readinessEntry in _readinessService.GetReadinessEntriesByIds(ids.ToArray()))
                    {
                        dictionary[readinessEntry.Id] = readinessEntry.ProductId;
                    }
                });

                result = dictionary;
            }

            return result;
        }
    }
}