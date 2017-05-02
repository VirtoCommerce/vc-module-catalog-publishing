using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogPublishingModule.Core.Model
{
    /// <summary>
    /// Readiness detail is a value of readiness per feature, i.e. readiness of product properties, readiness of product prices, etc.
    /// </summary>
    public class ReadinessDetail : ValueObject<ReadinessDetail>
    {
        public string Name { get; set; }

        public string ProductId { get; set; }

        public int ReadinessPercent { get; set; }
    }
}