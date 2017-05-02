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
                    var missedDescriptionTypes = descriptionTypes.Except(x.Reviews.Where(r => r.LanguageCode == channel.Language && !string.IsNullOrEmpty(r.Content))
                        .Select(t => t.ReviewType)
                        .Distinct());
                    detail.ReadinessPercent = ReadinessHelper.CalculateReadiness(descriptionTypes.Length, missedDescriptionTypes.Count());
                }
                return detail;
            }).ToArray();
        }
    }
}