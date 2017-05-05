using System.ComponentModel.DataAnnotations;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogPublishingModule.Data.Model
{
    public class ReadinessChannelCurrencyEntity : Entity
    {
        [Required]
        [StringLength(64)]
        public string CurrencyCode { get; set; }

        #region Navigation Properties

        public string ChannelId { get; set; }

        public ReadinessChannelEntity Channel { get; set; }

        #endregion
    }
}