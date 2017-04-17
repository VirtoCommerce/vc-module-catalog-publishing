using System.ComponentModel.DataAnnotations;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogPublishingModule.Data.Model
{
    public class ReadinessDetail : Entity
    {
        [Required]
        [StringLength(128)]
        public string Name { get; set; }
        
        [Range(0, 100)]
        public int ReadinessPercent { get; set; }

        #region Navigation properties

        public string ReadinessEntryId { get; set; }

        public virtual ReadinessEntry ReadinessEntry { get; set; }

        #endregion
    }
}