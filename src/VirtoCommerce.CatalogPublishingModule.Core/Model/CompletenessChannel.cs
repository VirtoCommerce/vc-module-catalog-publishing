using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogPublishingModule.Core.Model
{
    /// <summary>
    /// Completeness channel is a provider of completeness of specified catalog
    /// </summary>
    public class CompletenessChannel : AuditableEntity
    {
        public string Name { get; set; }

        public ICollection<string> Languages { get; set; }

        public ICollection<string> Currencies { get; set; }

        public string CatalogId { get; set; }

        public string CatalogName { get; set; }

        public string EvaluatorType { get; set; }

        public int CompletenessPercent { get; set; }
    }
}