using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using VirtoCommerce.CatalogPublishingModule.Core.Model;
using VirtoCommerce.CatalogPublishingModule.Core.Model.Search;
using VirtoCommerce.CatalogPublishingModule.Core.Services;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Commerce.Model.Search;
using VirtoCommerce.Platform.Core.PushNotifications;

namespace VirtoCommerce.CatalogPublishingModule.Web.Controllers.Api
{
    [RoutePrefix("api/readiness")]
    public class ReadinessController : ApiController
    {
        private readonly IReadinessService _readinessService;
        private readonly IReadinessEvaluator _readinessEvaluator;

        public ReadinessController(IReadinessService readinessService, IReadinessEvaluator readinessEvaluator)
        {
            _readinessService = readinessService;
            _readinessEvaluator = readinessEvaluator;
        }

        /// <summary>
        /// Get all available evaluators
        /// </summary>
        [HttpGet]
        [Route("evaluators")]
        [ResponseType(typeof(IReadinessEvaluator))]
        public IHttpActionResult GetEvaluators()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Evaluate readiness
        /// </summary>
        [HttpPost]
        [Route("channels/{id}/evaluate")]
        [ResponseType(typeof(PushNotification))]
        public IHttpActionResult EvaluateChannel(string id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Evaluate readiness
        /// </summary>
        /// <remarks>Evaluate readiness for specified products</remarks>
        [HttpPost]
        [Route("channels/{id}/products/evaluate")]
        [ResponseType(typeof(PushNotification))]
        public IHttpActionResult EvaluateChannel(string id, [FromBody] CatalogProduct[] products)
        {
            throw new NotImplementedException();
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
    }
}
