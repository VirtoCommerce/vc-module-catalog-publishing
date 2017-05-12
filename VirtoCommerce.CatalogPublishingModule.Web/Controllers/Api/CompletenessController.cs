using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using Hangfire;
using VirtoCommerce.CatalogPublishingModule.Core.Model;
using VirtoCommerce.CatalogPublishingModule.Core.Model.Search;
using VirtoCommerce.CatalogPublishingModule.Core.Services;
using VirtoCommerce.CatalogPublishingModule.Web.Model;
using VirtoCommerce.CatalogPublishingModule.Web.Security;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Domain.Commerce.Model.Search;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.PushNotifications;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Web.Security;
using VirtoCommerce.Platform.Data.Common;

namespace VirtoCommerce.CatalogPublishingModule.Web.Controllers.Api
{
    [RoutePrefix("api/completeness")]
    public class CompletenessController : ApiController
    {
        private readonly ICompletenessService _completenessService;
        private readonly ICompletenessEvaluator[] _completenessEvaluators;
        private readonly ICatalogSearchService _catalogSearchService;
        private readonly IItemService _productService;
        private readonly IUserNameResolver _userNameResolver;
        private readonly IPushNotificationManager _pushNotifier;

        public CompletenessController(ICompletenessService completenessService, ICompletenessEvaluator[] completenessEvaluators,
            ICatalogSearchService catalogSearchService, IItemService productService,
            IUserNameResolver userNameResolver, IPushNotificationManager pushNotifier)
        {
            _completenessService = completenessService;
            _completenessEvaluators = completenessEvaluators;
            _catalogSearchService = catalogSearchService;
            _productService = productService;
            _userNameResolver = userNameResolver;
            _pushNotifier = pushNotifier;
        }

        /// <summary>
        /// Get all available evaluators
        /// </summary>
        [HttpGet]
        [Route("evaluators")]
        [ResponseType(typeof(string[]))]
        public IHttpActionResult GetEvaluators()
        {
            return Ok(_completenessEvaluators.Select(x => x.GetType().Name));
        }

        /// <summary>
        /// Evaluate completeness 
        /// </summary>
        /// <remarks>Evaluate completeness for specified channel. Result will be saved to database.</remarks>
        [HttpPost]
        [Route("channels/{id}/evaluate")]
        [ResponseType(typeof(PushNotification))]
        [CheckPermission(Permission = ChannelPredefinedPermissions.Update)]
        public IHttpActionResult EvaluateCompleteness(string id)
        {
            return EvaluateCompleteness("EvaluateCompleteness", "Evaluate completeness task", notification => BackgroundJob.Enqueue(() => EvaluateCompletenessJob(id, notification)));
        }

        /// <summary>
        /// Evaluate completeness
        /// </summary>
        /// <remarks>Evaluate completeness for specified products</remarks>
        [HttpPost]
        [Route("channels/{id}/products/evaluate")]
        [ResponseType(typeof(CompletenessEntry[]))]
        public IHttpActionResult EvaluateCompleteness(string id, [FromBody] string[] productIds)
        {
            var channel = _completenessService.GetChannelsByIds(new[] { id }).FirstOrDefault();
            if (channel == null)
            {
                throw new ArgumentException("Channel with specified ID not found", nameof(id));
            }

            var evaluator = _completenessEvaluators.FirstOrDefault(x => channel.EvaluatorType == x.GetType().Name);
            if (evaluator == null)
            {
                throw new InvalidOperationException("Channel's evaluator type not found");
            }

            if (productIds.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(productIds));
            }

            var products = _productService.GetByIds(productIds, ItemResponseGroup.ItemInfo);
            if (products == null)
            {
                throw new ArgumentException("Products with specified IDs not found", nameof(productIds));
            }

            return Ok(evaluator.EvaluateCompleteness(channel, products));
        }

        /// <summary>
        /// Save evaluated completeness
        /// </summary>
        /// <param name="entries">Evaluated entries</param>
        [HttpPut]
        [Route("entries")]
        [ResponseType(typeof(void))]
        [CheckPermission(Permission = ChannelPredefinedPermissions.Update)]
        public IHttpActionResult SaveEntries(CompletenessEntry[] entries)
        {
            _completenessService.SaveEntries(entries);
            return StatusCode(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Get channel
        /// </summary>
        /// <param name="id">Channel id</param>
        [HttpGet]
        [Route("channels/{id}")]
        [ResponseType(typeof(CompletenessChannel))]
        [CheckPermission(Permission = ChannelPredefinedPermissions.Read)]
        public IHttpActionResult GetChannel(string id)
        {
            var channel = _completenessService.GetChannelsByIds(new[] { id }).FirstOrDefault();
            if (channel == null)
            {
                return NotFound();
            }
            return Ok(channel);
        }

        /// <summary>
        /// Search channels
        /// </summary>
        /// <remarks>Search channels by given criteria</remarks>
        [HttpPost]
        [Route("channels/search")]
        [ResponseType(typeof(GenericSearchResult<CompletenessChannel>))]
        [CheckPermission(Permission = ChannelPredefinedPermissions.Read)]
        public IHttpActionResult SearchChannel(CompletenessChannelSearchCriteria criteria)
        {
            if (criteria == null)
            {
                criteria = new CompletenessChannelSearchCriteria();
            }
            var result = _completenessService.SearchChannels(criteria);
            return Ok(result);
        }

        /// <summary>
        /// Create new channel
        /// </summary>
        /// <param name="channel">Channel</param>
        [HttpPost]
        [Route("channels")]
        [ResponseType(typeof(CompletenessChannel))]
        [CheckPermission(Permission = ChannelPredefinedPermissions.Create)]
        public IHttpActionResult CreateChannel(CompletenessChannel channel)
        {
            _completenessService.SaveChannels(new[] { channel });
            return Ok(channel);
        }

        /// <summary>
        /// Update channel
        /// </summary>
        /// <param name="channel">Channel</param>
        [HttpPut]
        [Route("channels")]
        [ResponseType(typeof(void))]
        [CheckPermission(Permission = ChannelPredefinedPermissions.Update)]
        public IHttpActionResult UpdateChannel(CompletenessChannel channel)
        {
            _completenessService.SaveChannels(new[] { channel });
            return StatusCode(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Delete channels  
        /// </summary>
        /// <remarks>Delete channels by given array of pricelist ids.</remarks>
        /// <param name="ids">An array of channels ids</param>
        [HttpDelete]
        [Route("channels")]
        [ResponseType(typeof(void))]
        [CheckPermission(Permission = ChannelPredefinedPermissions.Delete)]
        public IHttpActionResult DeleteChannels([FromUri] string[] ids)
        {
            _completenessService.DeleteChannels(ids);
            return StatusCode(HttpStatusCode.NoContent);
        }

        private IHttpActionResult EvaluateCompleteness(string notifyType, string notificationDescription, Action<EvaluateCompletenessNotification> job)
        {
            var notification = new EvaluateCompletenessNotification(_userNameResolver.GetCurrentUserName(), notifyType)
            {
                Title = notificationDescription,
                Description = "Starting evaluation..."
            };
            _pushNotifier.Upsert(notification);

            job(notification);

            return Ok(notification);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public void EvaluateCompletenessJob(string channelId, EvaluateCompletenessNotification notification)
        {
            var channel = _completenessService.GetChannelsByIds(new[] { channelId }).FirstOrDefault();
            if (channel == null)
            {
                throw new ArgumentException("Channel with specified ID not found", nameof(channelId));
            }

            var evaluator = _completenessEvaluators.FirstOrDefault(x => channel.EvaluatorType == x.GetType().Name);
            if (evaluator == null)
            {
                throw new InvalidOperationException("Channel's evaluator type not found");
            }

            try
            {
                const int productsPerIterationCount = 50;
                notification.TotalCount = _catalogSearchService
                    .Search(new SearchCriteria { CatalogId = channel.CatalogId, SearchInChildren = true, ResponseGroup = SearchResponseGroup.WithProducts, Take = 0 })
                    .ProductsTotalCount;
                do
                {
                    var products = _catalogSearchService
                        .Search(new SearchCriteria
                        {
                            CatalogId = channel.CatalogId,
                            SearchInChildren = true,
                            ResponseGroup = SearchResponseGroup.WithProducts,
                            Skip = (int)notification.ProcessedCount,
                            Take = productsPerIterationCount
                        }).Products;

                    var entries = evaluator.EvaluateCompleteness(channel, products.ToArray());
                    notification.Completeness = entries;
                    _completenessService.SaveEntries(entries);

                    notification.ProcessedCount += products.Count;
                    _pushNotifier.Upsert(notification);
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
                _pushNotifier.Upsert(notification);
            }
        }
    }
}
