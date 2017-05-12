using System;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogPublishingModule.Core.Model
{
    /// <summary>
    /// Completeness entriy is a value of completeness per product
    /// </summary>
    public class CompletenessEntry: ValueObject<CompletenessEntry>, IEntity, IAuditable
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

        public int CompletenessPercent { get; set; }

        public CompletenessDetail[] Details { get; set; }
    }
}