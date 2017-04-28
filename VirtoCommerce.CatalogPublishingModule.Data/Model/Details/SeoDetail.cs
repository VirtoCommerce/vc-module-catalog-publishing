using System.Linq;
using System.Text.RegularExpressions;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Pricing.Model;

namespace VirtoCommerce.CatalogPublishingModule.Data.Model.Details
{
    public class SeoDetail : DefaultReadinessDetail
    {
        public SeoDetail()
        {
            Name = "Seo";
        }

        public override void Evaluate(CatalogProduct product, string pricelistId, Price[] productPrices, string language)
        {
            string pattern = @"[$+;=%{}[\]|\\\/@ ~#!^*&?:'<>,]";
            ReadinessPercent = product.SeoInfos.Any(x => x.LanguageCode == language && !string.IsNullOrEmpty(x.SemanticUrl) && !Regex.IsMatch(x.SemanticUrl, pattern)) ? 100 : 0;
        }
    }
}