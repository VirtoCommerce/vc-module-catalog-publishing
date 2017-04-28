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
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Domain.Commerce.Model.Search;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.PushNotifications;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Data.Common;

namespace VirtoCommerce.CatalogPublishingModule.Web.Controllers.Api
{
    [RoutePrefix("api/readiness")]
    public class ReadinessController : ApiController
    {
        private readonly IReadinessService _readinessService;
        private readonly IReadinessEvaluator[] _readinessEvaluators;
        private readonly ICatalogSearchService _catalogSearchService;
        private readonly IItemService _productService;
        private readonly IUserNameResolver _userNameResolver;
        private readonly IPushNotificationManager _pushNotifier;

        public ReadinessController(IReadinessService readinessService, IReadinessEvaluator[] readinessEvaluators,
            ICatalogSearchService catalogSearchService, IItemService productService,
            IUserNameResolver userNameResolver, IPushNotificationManager pushNotifier)
        {
            _readinessService = readinessService;
            _readinessEvaluators = readinessEvaluators;
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
            return Ok(_readinessEvaluators.Select(x => x.GetType().Name));
        }

        /// <summary>
        /// Evaluate readiness
        /// </summary>
        [HttpPost]
        [Route("channels/{id}/evaluate")]
        [ResponseType(typeof(PushNotification))]
        public IHttpActionResult EvaluateReadiness(string id)
        {
            return EvaluateReadiness("EvaluateReadiness", "Evaluate readiness task", notification => BackgroundJob.Enqueue(() => EvaluateReadinessJob(id, null, notification)));
        }

        /// <summary>
        /// Evaluate readiness
        /// </summary>
        /// <remarks>Evaluate readiness for specified products</remarks>
        [HttpPost]
        [Route("channels/{id}/products/evaluate")]
        [ResponseType(typeof(ReadinessEntry[]))]
        public IHttpActionResult EvaluateReadiness(string id, [FromBody] string[] productIds)
        {
            return Ok(EvaluateReadinessWorker(id, productIds));
        }

        /// <summary>
        /// Get channel
        /// </summary>
        /// <param name="id">Channel id</param>
        [HttpGet]
        [Route("channels/{id}")]
        [ResponseType(typeof(ReadinessChannel))]
        public IHttpActionResult GetChannel(string id)
        {
            var channel = _readinessService.GetChannelsByIds(new[] { id }).FirstOrDefault();
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
        [ResponseType(typeof(GenericSearchResult<ReadinessChannel>))]
        public IHttpActionResult SearchChannel(ReadinessChannelSearchCriteria criteria)
        {
            if (criteria == null)
            {
                criteria = new ReadinessChannelSearchCriteria();
            }
            var result = _readinessService.SearchChannels(criteria);
            return Ok(result);
        }

        /// <summary>
        /// Create new channel
        /// </summary>
        /// <param name="channel">Channel</param>
        [HttpPost]
        [Route("channels")]
        [ResponseType(typeof(ReadinessChannel))]
        public IHttpActionResult CreateChannel(ReadinessChannel channel)
        {
            _readinessService.SaveChannels(new[] { channel });
            return Ok(channel);
        }

        /// <summary>
        /// Update channel
        /// </summary>
        /// <param name="channel">Channel</param>
        [HttpPut]
        [Route("channels")]
        [ResponseType(typeof(void))]
        public IHttpActionResult UpdateChannel(ReadinessChannel channel)
        {
            _readinessService.SaveChannels(new[] { channel });
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
        public IHttpActionResult DeleteChannels([FromUri] string[] ids)
        {
            _readinessService.DeleteChannels(ids);
            return StatusCode(HttpStatusCode.NoContent);
        }

        private IHttpActionResult EvaluateReadiness(string notifyType, string notificationDescription, Action<EvaluateReadinessNotification> job)
        {
            var notification = new EvaluateReadinessNotification(_userNameResolver.GetCurrentUserName(), notifyType)
            {
                Title = notificationDescription,
                Description = "Starting evaluation..."
            };
            _pushNotifier.Upsert(notification);

            job(notification);

            return Ok(notification);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public void EvaluateReadinessJob(string channelId, string[] productIds, EvaluateReadinessNotification notification)
        {
            try
            {
                notification.Readiness = EvaluateReadinessWorker(channelId, productIds);
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

        private ReadinessEntry[] EvaluateReadinessWorker(string channelId, string[] productIds)
        {
            var channel = _readinessService.GetChannelsByIds(new[] { channelId }).FirstOrDefault();
            if (channel == null)
            {
                throw new NullReferenceException("channel");
            }
            if (productIds.IsNullOrEmpty())
            {
                productIds = _catalogSearchService.Search(new SearchCriteria
                    {
                        CatalogId = channel.CatalogId, SearchInChildren = true, ResponseGroup = SearchResponseGroup.WithProducts, Take = int.MaxValue
                    }).Products.Select(x => x.Id).ToArray();
            }
            var products = _productService.GetByIds(productIds, ItemResponseGroup.ItemInfo);
            if (products == null)
            {
                throw new NullReferenceException("products");
            }

            var evaluator = _readinessEvaluators.FirstOrDefault(x => channel.EvaluatorType == x.GetType().Name);
            if (evaluator == null)
            {
                throw new InvalidOperationException("Channel's evaluator type not found");
            }
            return evaluator.EvaluateReadiness(channel, products);
        }
    }
}
