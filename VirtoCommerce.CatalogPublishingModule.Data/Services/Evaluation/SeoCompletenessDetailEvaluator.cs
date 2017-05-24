using System.Linq;
using System.Text.RegularExpressions;
using VirtoCommerce.CatalogPublishingModule.Core.Model;
using VirtoCommerce.CatalogPublishingModule.Core.Services;
using VirtoCommerce.CatalogPublishingModule.Data.Common;
using VirtoCommerce.Domain.Catalog.Model;

namespace VirtoCommerce.CatalogPublishingModule.Data.Services.Evaluation
{
    /// <summary>
    /// Properties validator. Check that products have at least one SEO per channel language,
    /// URL keyword of which is not null or empty and does not contain invalid symbols (where invalid symbols is $+;=%{}[]|\/@ ~#!^*&?:'&lt;&gt;,)
    /// </summary>
    public class SeoCompletenessDetailEvaluator : ICompletenessDetailEvaluator
    {
        public CompletenessDetail[] EvaluateCompleteness(CompletenessChannel channel, CatalogProduct[] products)
        {
            var pattern = @"[$+;=%{}[\]|\\\/@ ~#!^*&?:'<>,]";
            return products.Select(p =>
            {
                var languagesWithoutValidSeoInfo = channel.Languages.Where(l => {
                    var seoInfosForLanguage = p.SeoInfos?.Where(si => si.LanguageCode == l).ToArray();
                    return seoInfosForLanguage == null || !seoInfosForLanguage.Any() || seoInfosForLanguage.All(si => string.IsNullOrEmpty(si.SemanticUrl) || Regex.IsMatch(si.SemanticUrl, pattern));
                });
                var detail = new CompletenessDetail
                {
                    Name = "Seo",
                    ProductId = p.Id,
                    CompletenessPercent = CompletenessHelper.CalculateCompleteness(channel.Languages.Count, languagesWithoutValidSeoInfo.Count())
                };
                return detail;
            }).ToArray();
        }
    }
}