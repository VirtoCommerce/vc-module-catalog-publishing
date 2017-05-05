using System.Linq;
using VirtoCommerce.CatalogPublishingModule.Core.Model;
using VirtoCommerce.CatalogPublishingModule.Data.Common;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.CatalogPublishingModule.Data.Services.Evaluators
{
    public class DescriptionsReadinessDetailEvaluator : DefaultReadinessDetailEvaluator
    {
        private readonly ISettingsManager _settingsManager;

        public DescriptionsReadinessDetailEvaluator(ISettingsManager settingsManager)
        {
            _settingsManager = settingsManager;
        }

        public override ReadinessDetail[] EvaluateReadiness(ReadinessChannel channel, CatalogProduct[] products)
        {
            var descriptionTypes = _settingsManager.GetSettingByName("Catalog.EditorialReviewTypes").ArrayValues;
            return products.Select(x =>
            {
                var detail = new ReadinessDetail { Name = "Descriptions", ProductId = x.Id };
                if (descriptionTypes.IsNullOrEmpty())
                {
                    detail.ReadinessPercent = 100;
                }
                else
                {
                    var missedDescriptionTypesPerLanguageCount = channel.Languages
                        .Select(l => descriptionTypes.Except(x.Reviews.Where(r => r.LanguageCode == l && !string.IsNullOrEmpty(r.Content)).Select(r => r.ReviewType).Distinct()).Count())
                        .Sum();
                    detail.ReadinessPercent = ReadinessHelper.CalculateReadiness(descriptionTypes.Length * channel.Languages.Count, missedDescriptionTypesPerLanguageCount);
                }
                return detail;
            }).ToArray();
        }
    }
}