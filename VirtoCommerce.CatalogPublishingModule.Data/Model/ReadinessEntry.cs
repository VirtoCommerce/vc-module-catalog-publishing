using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogPublishingModule.Data.Model
{
    public class ReadinessEntry : Entity
    {
        public ReadinessEntry()
        {
            Details = new NullCollection<ReadinessDetail>();
        }

        [Required]
        [StringLength(128)]
        public string ProductId { get; set; }
        
        [Range(0, 100)]
        public int ReadinessPercent { get; set; }

        public ObservableCollection<ReadinessDetail> Details { get; set; }

        #region Navigation properties
        
        public string ChannelId { get; set; }

        public virtual ReadinessChannel Channel { get; set; }

        #endregion

    }
}