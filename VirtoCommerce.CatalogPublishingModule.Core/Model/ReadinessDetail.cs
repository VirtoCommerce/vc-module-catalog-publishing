using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogPublishingModule.Core.Model
{
    public class ReadinessDetail : ValueObject<ReadinessDetail>
    {
        public string Name { get; set; }

        public int ReadinessPercent { get; set; }
    }
}