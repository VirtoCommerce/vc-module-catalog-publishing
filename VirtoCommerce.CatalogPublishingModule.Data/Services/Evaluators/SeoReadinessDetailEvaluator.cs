using System.Linq;
using System.Text.RegularExpressions;
using VirtoCommerce.CatalogPublishingModule.Core.Model;
using VirtoCommerce.CatalogPublishingModule.Data.Common;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogPublishingModule.Data.Services.Evaluators
{
    public class SeoReadinessDetailEvaluator : DefaultReadinessDetailEvaluator
    {
        public override ReadinessDetail[] EvaluateReadiness(ReadinessChannel channel, CatalogProduct[] products)
        {
            var pattern = @"[$+;=%{}[\]|\\\/@ ~#!^*&?:'<>,]";
            return products.Select(p =>
            {
                var languagesWithoutValidSeoInfo = channel.Languages.Where(l => {
                    var seoInfosForLanguage = p.SeoInfos?.Where(si => si.LanguageCode == l).ToArray();
                    return seoInfosForLanguage == null || !seoInfosForLanguage.Any() || seoInfosForLanguage.All(si => string.IsNullOrEmpty(si.SemanticUrl) || Regex.IsMatch(si.SemanticUrl, pattern));
                });
                var detail = new ReadinessDetail
                {
                    Name = "Seo",
                    ProductId = p.Id,
                    ReadinessPercent = ReadinessHelper.CalculateReadiness(channel.Languages.Count, languagesWithoutValidSeoInfo.Count())
                };
                return detail;
            }).ToArray();
        }
    }
}