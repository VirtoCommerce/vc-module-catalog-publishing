using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogPublishingModule.Core;
using VirtoCommerce.CatalogPublishingModule.Core.Model;
using VirtoCommerce.CatalogPublishingModule.Core.Model.Search;
using VirtoCommerce.CatalogPublishingModule.Core.Services;
using VirtoCommerce.CatalogPublishingModule.Web.BackgroundJobs;
using VirtoCommerce.CatalogPublishingModule.Web.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Jobs;
using VirtoCommerce.Platform.Core.PushNotifications;
using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.CatalogPublishingModule.Web.Controllers.Api
{
    [Route("api/completeness")]
    public class CompletenessController : Controller
    {
        private readonly ICompletenessService _completenessService;
        private readonly ICompletenessEvaluator[] _completenessEvaluators;
        private readonly IProductIndexedSearchService _productIndexedSearchService;
        private readonly IItemService _productService;
        private readonly IUserNameResolver _userNameResolver;
        private readonly IPushNotificationManager _pushNotifier;
        private readonly IBackgroundJob _backgroundJob;

        public CompletenessController(ICompletenessService completenessService,
            IEnumerable<ICompletenessEvaluator> completenessEvaluators,
            IProductIndexedSearchService productIndexedSearchService,
            IItemService productService,
            IUserNameResolver userNameResolver,
            IPushNotificationManager pushNotifier,
            IBackgroundJob backgroundJob)
        {
            _completenessService = completenessService;
            _completenessEvaluators = completenessEvaluators.ToArray();
            _productIndexedSearchService = productIndexedSearchService;
            _productService = productService;
            _userNameResolver = userNameResolver;
            _pushNotifier = pushNotifier;
            _backgroundJob = backgroundJob;
        }

        /// <summary>
        /// Get all available evaluators
        /// </summary>
        [HttpGet]
        [Route("evaluators")]
        [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
        public Task<ActionResult<string[]>> GetEvaluators()
        {
            var result = (_completenessEvaluators.Select(x => x.GetType().Name).ToArray());

            return Task.FromResult<ActionResult<string[]>>(Ok(result));
        }

        /// <summary>
        /// Evaluate completeness 
        /// </summary>
        /// <remarks>Evaluate completeness for specified channel. Result will be saved to database.</remarks>
        [HttpPost]
        [Route("channels/{id}/evaluate")]
        [ProducesResponseType(typeof(EvaluateCompletenessNotification), StatusCodes.Status200OK)]
        [Authorize(ModuleConstants.Security.Permissions.Update)]
        public async Task<ActionResult<EvaluateCompletenessNotification>> EvaluateChannelCompletenessAsync([FromRoute] string id)
        {
            return await EvaluateCompletenessAsync("EvaluateCompleteness", "Evaluate completeness task", notification => _backgroundJob.Enqueue<EvaluateCompletenessJobHandler>(CreatePayload(id, notification)));
        }

        /// <summary>
        /// Evaluate completeness
        /// </summary>
        /// <remarks>Evaluate completeness for specified products</remarks>
        [HttpPost]
        [Route("channels/{id}/products/evaluate")]
        [ProducesResponseType(typeof(CompletenessEntry[]), StatusCodes.Status200OK)]
        public async Task<ActionResult> EvaluateProductsCompleteness([FromRoute] string id, [FromBody] string[] productIds)
        {
            var channel = (await _completenessService.GetChannelsByIdsAsync(new[] { id })).FirstOrDefault();

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

            var products = await _productService.GetAsync(productIds, ItemResponseGroup.ItemInfo.ToString());
            if (products == null)
            {
                throw new ArgumentException("Products with specified IDs not found", nameof(productIds));
            }

            var result = await evaluator.EvaluateCompletenessAsync(channel, products.ToArray());

            return Ok(result);
        }

        /// <summary>
        /// Save evaluated completeness
        /// </summary>
        /// <param name="entries">Evaluated entries</param>
        [HttpPut]
        [Route("entries")]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        [Authorize(ModuleConstants.Security.Permissions.Update)]
        public async Task<ActionResult> SaveEntries([FromBody] CompletenessEntry[] entries)
        {
            await _completenessService.SaveEntriesAsync(entries);

            return NoContent();
        }

        /// <summary>
        /// Get channel
        /// </summary>
        /// <param name="id">Channel id</param>
        [HttpGet]
        [Route("channels/{id}")]
        [ProducesResponseType(typeof(CompletenessChannel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
        [Authorize(ModuleConstants.Security.Permissions.Read)]
        public async Task<ActionResult<CompletenessChannel>> GetChannel([FromRoute] string id)
        {
            var channel = (await _completenessService.GetChannelsByIdsAsync(new[] { id })).FirstOrDefault();

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
        [ProducesResponseType(typeof(CompletenessChannelSearchResult), StatusCodes.Status200OK)]
        [Authorize(ModuleConstants.Security.Permissions.Read)]
        public async Task<ActionResult<CompletenessChannelSearchResult>> SearchChannel([FromBody] CompletenessChannelSearchCriteria criteria)
        {
            if (criteria == null)
            {
                criteria = new CompletenessChannelSearchCriteria();
            }

            var result = await _completenessService.SearchChannelsAsync(criteria);

            return Ok(result);
        }

        /// <summary>
        /// Create new channel
        /// </summary>
        /// <param name="channel">Channel</param>
        [HttpPost]
        [Route("channels")]
        [ProducesResponseType(typeof(CompletenessChannel), StatusCodes.Status200OK)]
        [Authorize(ModuleConstants.Security.Permissions.Create)]
        public async Task<ActionResult<CompletenessChannel>> CreateChannel([FromBody] CompletenessChannel channel)
        {
            await _completenessService.SaveChannelsAsync(new[] { channel });

            return Ok(channel);
        }

        /// <summary>
        /// Update channel
        /// </summary>
        /// <param name="channel">Channel</param>
        [HttpPut]
        [Route("channels")]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        [Authorize(ModuleConstants.Security.Permissions.Update)]
        public async Task<ActionResult> UpdateChannel([FromBody] CompletenessChannel channel)
        {
            await _completenessService.SaveChannelsAsync(new[] { channel });

            return NoContent();
        }

        /// <summary>
        /// Delete channels  
        /// </summary>
        /// <remarks>Delete channels by given array of pricelist ids.</remarks>
        /// <param name="ids">An array of channels ids</param>
        [HttpDelete]
        [Route("channels")]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        [Authorize(ModuleConstants.Security.Permissions.Delete)]
        public async Task<ActionResult> DeleteChannels([FromQuery] string[] ids)
        {
            await _completenessService.DeleteChannelsAsync(ids);

            return NoContent();
        }

        private async Task<ActionResult<EvaluateCompletenessNotification>> EvaluateCompletenessAsync(string notifyType, string notificationDescription, Func<EvaluateCompletenessNotification, Task> job)
        {
            var notification = new EvaluateCompletenessNotification(_userNameResolver.GetCurrentUserName(), notifyType)
            {
                Title = notificationDescription,
                Description = "Starting evaluation..."
            };
            await _pushNotifier.SendAsync(notification);

            await job(notification);

            return Ok(notification);
        }

        /// <summary>
        /// Kept for background jobs enqueued by an earlier version, which reference this method by name.
        /// Hands the work to <see cref="EvaluateCompletenessJobHandler"/>; remove this once no such job can still be pending.
        /// </summary>
        // Signature is byte-identical on purpose: Hangfire persists a queued job as type name + method name +
        // parameter types + serialized args, so changing any of them would strand already-queued entries as Failed.
        [ApiExplorerSettings(IgnoreApi = true)]
        [Obsolete("Enqueued indirectly by legacy Hangfire jobs only; new work uses EvaluateCompletenessJobHandler.", DiagnosticId = "VC0015", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
        public Task EvaluateCompletenessJob(string channelId, EvaluateCompletenessNotification notification)
        {
            // The static facade: Hangfire activates this controller outside a request to run the legacy job.
            return BackgroundJob.Enqueue<EvaluateCompletenessJobHandler>(CreatePayload(channelId, notification));
        }

        private static EvaluateCompletenessJobPayload CreatePayload(string channelId, EvaluateCompletenessNotification notification)
        {
            var payload = AbstractTypeFactory<EvaluateCompletenessJobPayload>.TryCreateInstance();
            payload.ChannelId = channelId;
            payload.Notification = notification;
            return payload;
        }
    }
}
