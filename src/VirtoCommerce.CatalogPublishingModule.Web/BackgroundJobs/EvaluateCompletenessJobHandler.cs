using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogPublishingModule.Core.Services;
using VirtoCommerce.CatalogPublishingModule.Web.Model;
using VirtoCommerce.Platform.Core.Exceptions;
using VirtoCommerce.Platform.Core.Jobs;
using VirtoCommerce.Platform.Core.PushNotifications;

namespace VirtoCommerce.CatalogPublishingModule.Web.BackgroundJobs
{
    public class EvaluateCompletenessJobPayload
    {
        public string ChannelId { get; set; }

        public EvaluateCompletenessNotification Notification { get; set; }
    }

    /// <summary>
    /// Evaluates completeness for every product of a channel's catalog, saves the entries, and reports progress
    /// through the push notification returned to the admin UI when the evaluation was started.
    /// </summary>
    public class EvaluateCompletenessJobHandler : IBackgroundJobHandler<EvaluateCompletenessJobPayload>
    {
        private const int ProductsPerIterationCount = 50;

        private readonly ICompletenessService _completenessService;
        private readonly ICompletenessEvaluator[] _completenessEvaluators;
        private readonly IProductIndexedSearchService _productIndexedSearchService;
        private readonly IPushNotificationManager _pushNotifier;

        public EvaluateCompletenessJobHandler(
            ICompletenessService completenessService,
            IEnumerable<ICompletenessEvaluator> completenessEvaluators,
            IProductIndexedSearchService productIndexedSearchService,
            IPushNotificationManager pushNotifier)
        {
            _completenessService = completenessService;
            _completenessEvaluators = completenessEvaluators.ToArray();
            _productIndexedSearchService = productIndexedSearchService;
            _pushNotifier = pushNotifier;
        }

        public virtual async Task Execute(EvaluateCompletenessJobPayload payload, IJobExecutionContext context, CancellationToken cancellationToken = default)
        {
            var channelId = payload.ChannelId;
            var notification = payload.Notification;

            var channel = (await _completenessService.GetChannelsByIdsAsync(new[] { channelId })).FirstOrDefault();
            if (channel == null)
            {
                throw new ArgumentException("Channel with specified ID not found", nameof(payload));
            }

            var evaluator = _completenessEvaluators.FirstOrDefault(x => channel.EvaluatorType == x.GetType().Name);
            if (evaluator == null)
            {
                throw new InvalidOperationException("Channel's evaluator type not found");
            }

            try
            {
                notification.TotalCount = (await _productIndexedSearchService
                    .SearchAsync(new ProductIndexedSearchCriteria { CatalogId = channel.CatalogId, ResponseGroup = ItemResponseGroup.ItemInfo.ToString(), Take = 0 }))
                    .TotalCount;
                do
                {
                    var products = (await _productIndexedSearchService
                        .SearchAsync(new ProductIndexedSearchCriteria
                        {
                            CatalogId = channel.CatalogId,
                            ResponseGroup = ItemResponseGroup.ItemInfo.ToString(),
                            Skip = (int)notification.ProcessedCount,
                            Take = ProductsPerIterationCount
                        })).Items;

                    var entries = await evaluator.EvaluateCompletenessAsync(channel, products);
                    notification.Completeness = entries;
                    await _completenessService.SaveEntriesAsync(entries);

                    notification.ProcessedCount += products.Length;
                    await _pushNotifier.SendAsync(notification);
                } while (notification.ProcessedCount < notification.TotalCount);
            }
            catch (Exception ex)
            {
                notification.Description = "Evaluation failed";
                notification.Errors.Add(ex.ExpandExceptionMessage());
            }
            finally
            {
                notification.Description = "Evaluation finished";
                notification.Finished = DateTime.UtcNow;
                await _pushNotifier.SendAsync(notification);
            }
        }
    }
}
