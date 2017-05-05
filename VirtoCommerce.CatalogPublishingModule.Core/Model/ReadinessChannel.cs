using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogPublishingModule.Core.Model
{
    /// <summary>
    /// Readiness channel is a provider of readiness of specified catalog
    /// </summary>
    public class ReadinessChannel : AuditableEntity
    {
        public string Name { get; set; }

        public ICollection<string> Languages { get; set; }

        public ICollection<string> Currencies { get; set; }

        public string CatalogId { get; set; }

        public string CatalogName { get; set; }

        public string EvaluatorType { get; set; }

        public int ReadinessPercent { get; set; }
    }
}