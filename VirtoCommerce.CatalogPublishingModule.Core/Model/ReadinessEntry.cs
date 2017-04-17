namespace VirtoCommerce.CatalogPublishingModule.Core.Model
{
    public class ReadinessEntry
    {
        public string ChannelId { get; set; }

        public string ProductId { get; set; }

        public int ReadinessPercent { get; set; }

        public ReadinessDetail[] Details { get; set; }
    }
}