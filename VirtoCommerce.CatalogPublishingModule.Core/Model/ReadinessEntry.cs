using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogPublishingModule.Core.Model
{
    /// <summary>
    /// Readiness entriy is a value of readiness per product
    /// </summary>
    public class ReadinessEntry: ValueObject<ReadinessEntry>
    {
        public string ChannelId { get; set; }

        public string ProductId { get; set; }

        public int ReadinessPercent { get; set; }

        public ReadinessDetail[] Details { get; set; }
    }
}