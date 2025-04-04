using System;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogPublishingModule.Core.Model
{
    /// <summary>
    /// Completeness detail is a value of completeness per feature, i.e. completeness of product properties, completeness of product prices, etc.
    /// </summary>
    public class CompletenessDetail : AuditableEntity, ICloneable
    {
        public string Name { get; set; }

        public string ProductId { get; set; }

        public decimal CompletenessPercent { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
