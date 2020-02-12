using System.Linq;
using VirtoCommerce.CatalogPublishingModule.Core.Model;
using VirtoCommerce.CatalogPublishingModule.Core.Services;
using VirtoCommerce.CatalogPublishingModule.Data.Common;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.CatalogPublishingModule.Data.Services.Evaluation
{
    /// <summary>
    ///  Descriptions valiator. Check that products have description per item description type per channel language, which content is not null or empty.
    /// </summary>
    public class DescriptionsCompletenessDetailEvaluator : ICompletenessDetailEvaluator
    {
        private readonly ISettingsManager _settingsManager;

        public DescriptionsCompletenessDetailEvaluator(ISettingsManager settingsManager)
        {
            _settingsManager = settingsManager;
        }

        public CompletenessDetail[] EvaluateCompleteness(CompletenessChannel channel, CatalogProduct[] products)
        {
            var descriptionTypes = _settingsManager.GetSettingByName("Catalog.EditorialReviewTypes").ArrayValues;
            return products.Select(x =>
            {
                var detail = new CompletenessDetail { Name = "Descriptions", ProductId = x.Id };
                if (descriptionTypes.IsNullOrEmpty())
                {
                    detail.CompletenessPercent = 100;
                }
                else
                {
                    var missedDescriptionTypesPerLanguageCount = channel.Languages
                        .Select(l => descriptionTypes.Except(x.Reviews.Where(r => r.LanguageCode == l && !string.IsNullOrEmpty(r.Content)).Select(r => r.ReviewType).Distinct()).Count())
                        .Sum();
                    detail.CompletenessPercent = CompletenessHelper.CalculateCompleteness(descriptionTypes.Length * channel.Languages.Count, missedDescriptionTypesPerLanguageCount);
                }
                return detail;
            }).ToArray();
        }
    }
}