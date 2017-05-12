using System;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogPublishingModule.Core.Model
{
    /// <summary>
    /// Readiness entriy is a value of readiness per product
    /// </summary>
    public class ReadinessEntry: ValueObject<ReadinessEntry>, IEntity, IAuditable
    {
        #region Implementation of IEntity

        public string Id { get; set; }

        #endregion

        #region Implementation of IAuditable

        public DateTime CreatedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public string CreatedBy { get; set; }

        public string ModifiedBy { get; set; }

        #endregion

        public string ChannelId { get; set; }

        public string ProductId { get; set; }

        public int ReadinessPercent { get; set; }

        public ReadinessDetail[] Details { get; set; }
    }
}