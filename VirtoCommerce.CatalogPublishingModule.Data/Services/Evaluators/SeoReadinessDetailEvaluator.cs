using System.Linq;
using System.Text.RegularExpressions;
using VirtoCommerce.CatalogPublishingModule.Core.Model;
using VirtoCommerce.Domain.Catalog.Model;

namespace VirtoCommerce.CatalogPublishingModule.Data.Services.Evaluators
{
    public class SeoReadinessDetailEvaluator : DefaultReadinessDetailEvaluator
    {
        public override ReadinessDetail[] EvaluateReadiness(ReadinessChannel channel, CatalogProduct[] products)
        {
            var pattern = @"[$+;=%{}[\]|\\\/@ ~#!^*&?:'<>,]";
            return products.Select(x => new ReadinessDetail
            {
                Name = "Seo",
                ProductId = x.Id,
                ReadinessPercent = x.SeoInfos.Any(s => s.LanguageCode == channel.Language && !string.IsNullOrEmpty(s.SemanticUrl) && !Regex.IsMatch(s.SemanticUrl, pattern)) ? 100 : 0
            }).ToArray();
        }
    }
}