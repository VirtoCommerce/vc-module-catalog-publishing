using System.Linq;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Pricing.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.CatalogPublishingModule.Data.Model.Details
{
    public class DescriptionsDetail : DefaultReadinessDetail
    {
        private readonly ISettingsManager _settingsManager;

        public DescriptionsDetail(ISettingsManager settingsManager)
        {
            _settingsManager = settingsManager;

            Name = "Descriptions";
        }

        public override void Evaluate(CatalogProduct product, string pricelistId, Price[] productPrices, string language)
        {
            int readinessPercent;
            var descriptionTypes = _settingsManager.GetSettingByName("Catalog.EditorialReviewTypes").ArrayValues;
            if (descriptionTypes.IsNullOrEmpty())
            {
                readinessPercent = 100;
            }
            else
            {
                var missedDescriptionTypes = descriptionTypes.Except(product.Reviews.Where(x => x.LanguageCode == language && !string.IsNullOrEmpty(x.Content)).Select(x => x.ReviewType).Distinct());
                readinessPercent = ReadinessHelper.CalculateReadiness(descriptionTypes.Length, missedDescriptionTypes.Count());
            }
            ReadinessPercent = readinessPercent;
        }
    }
}