using System;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogPublishingModule.Core.Model
{
    /// <summary>
    /// Completeness entriy is a value of completeness per product
    /// </summary>
    public class CompletenessEntry: AuditableEntity, ICloneable
    {

        public string ChannelId { get; set; }

        public string ProductId { get; set; }

        public decimal CompletenessPercent { get; set; }

        public CompletenessDetail[] Details { get; set; }
        public object Clone()
        {
            var result = (CompletenessEntry)MemberwiseClone();
            result.Details = Details.CloneTyped();

            return result;
        }
    }
}
