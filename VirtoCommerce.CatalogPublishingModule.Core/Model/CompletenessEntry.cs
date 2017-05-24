using System;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogPublishingModule.Core.Model
{
    /// <summary>
    /// Completeness entriy is a value of completeness per product
    /// </summary>
    public class CompletenessEntry: AuditableEntity
    {

        public string ChannelId { get; set; }

        public string ProductId { get; set; }

        public int CompletenessPercent { get; set; }

        public CompletenessDetail[] Details { get; set; }
    }
}