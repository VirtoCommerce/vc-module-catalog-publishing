using System.ComponentModel.DataAnnotations;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogPublishingModule.Data.Model
{
    public class ReadinessChannel : Entity
    {
        [Required]
        [StringLength(128)]
        public string Name { get; set; }
        
        [Required]
        [StringLength(128)]
        public string CatalogId { get; set; }

        public string Language { get; set; }

        [StringLength(128)]
        public string PricelistId { get; set; }

        public string EvaluatorType { get; set; }
    }
}