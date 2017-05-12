using System.ComponentModel.DataAnnotations;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogPublishingModule.Data.Model
{
    public class CompletenessChannelLanguageEntity : Entity
    {
        [Required]
        public string LanguageCode { get; set; }

        #region Navigation Properties

        public string ChannelId { get; set; }

        public CompletenessChannelEntity Channel { get; set; }

        #endregion
    }
}