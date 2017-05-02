using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogPublishingModule.Core.Model
{
    /// <summary>
    /// Readiness channel is a provider of readiness of specified catalog
    /// </summary>
    public class ReadinessChannel : AuditableEntity
    {
        public string Name { get; set; }

        public string Language { get; set; }

        public string PricelistId { get; set; }

        public string CatalogId { get; set; }

        public string CatalogName { get; set; }

        public string EvaluatorType { get; set; }

        public int ReadinessPercent { get; set; }
    }
}