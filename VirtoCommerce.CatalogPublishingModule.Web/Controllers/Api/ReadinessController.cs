using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
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
        /// <remarks>Evaluate readiness for specified channel. Result will be saved to database.</remarks>
        [HttpPost]
        [Route("channels/{id}/evaluate")]
        [ResponseType(typeof(PushNotification))]
        [CheckPermission(Permission = ChannelPredefinedPermissions.Evaluate)]
        public IHttpActionResult EvaluateReadiness(string id)
        {
            return EvaluateReadiness("EvaluateReadiness", "Evaluate readiness task", notification => BackgroundJob.Enqueue(() => EvaluateReadinessJob(id, notification)));
        }

        /// <summary>
        /// Evaluate readiness
        /// </summary>
        /// <remarks>Evaluate readiness for specified products</remarks>
        [HttpPost]
        [Route("channels/{id}/products/evaluate")]
        [ResponseType(typeof(ReadinessEntry[]))]
        [CheckPermission(Permission = ChannelPredefinedPermissions.Evaluate)]
        public IHttpActionResult EvaluateReadiness(string id, [FromBody] string[] productIds)
        {
            var channel = _readinessService.GetChannelsByIds(new[] { id }).FirstOrDefault();
            if (channel == null)
            {
                throw new ArgumentException("Channel with specified ID not found", nameof(id));
            }

            var evaluator = _readinessEvaluators.FirstOrDefault(x => channel.EvaluatorType == x.GetType().Name);
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

            return Ok(evaluator.EvaluateReadiness(channel, products));
        }

        /// <summary>
        /// Save evaluated readiness
        /// </summary>
        /// <param name="entries">Evaluated entries</param>
        [HttpPut]
        [Route("entries")]
        [ResponseType(typeof(void))]
        [CheckPermission(Permission = ChannelPredefinedPermissions.Evaluate)]
        public IHttpActionResult SaveEntries(ReadinessEntry[] entries)
        {
            _readinessService.SaveEntries(entries);
            return StatusCode(HttpStatusCode.NoContent);
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
        [CheckPermission(Permission = ChannelPredefinedPermissions.Create)]
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
        [CheckPermission(Permission = ChannelPredefinedPermissions.Update)]
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
        [CheckPermission(Permission = ChannelPredefinedPermissions.Delete)]
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
        public void EvaluateReadinessJob(string channelId, EvaluateReadinessNotification notification)
        {
            var channel = _readinessService.GetChannelsByIds(new[] { channelId }).FirstOrDefault();
            if (channel == null)
            {
                throw new ArgumentException("Channel with specified ID not found", nameof(channelId));
            }

            var evaluator = _readinessEvaluators.FirstOrDefault(x => channel.EvaluatorType == x.GetType().Name);
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

                    var entries = evaluator.EvaluateReadiness(channel, products.ToArray());
                    notification.Readiness = entries;
                    _readinessService.SaveEntries(entries);

                    notification.ProcessedCount += products.Count;
                    _pushNotifier.Upsert(notification);
                } while (notification.ProcessedCount < notification.TotalCount);
            }
            catch (Exception ex)
            {
                //notification.Description = "Evaluation failed";
                notification.Errors.Add(ex.ExpandExceptionMessage());
            }
            finally
            {
                //notification.Description = "Evaluation finished";
                notification.Finished = DateTime.UtcNow;
                _pushNotifier.Upsert(notification);
            }
        }
    }
}
