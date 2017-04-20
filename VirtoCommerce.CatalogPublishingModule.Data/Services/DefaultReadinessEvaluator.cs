using System;
using System.Linq;
using System.Text.RegularExpressions;
using VirtoCommerce.CatalogPublishingModule.Core.Model;
using VirtoCommerce.CatalogPublishingModule.Core.Services;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Domain.Pricing.Model;
using VirtoCommerce.Domain.Pricing.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.CatalogPublishingModule.Data.Services
{
    public class DefaultReadinessEvaluator : IReadinessEvaluator
    {
        private readonly IReadinessService _readinessService;
        private readonly IItemService _productService;
        private readonly IPricingService _pricingService;
        private readonly ISettingsManager _settingsManager;

        public DefaultReadinessEvaluator(IReadinessService readinessService, IItemService productService, IPricingService pricingService, ISettingsManager settingsManager)
        {
            _readinessService = readinessService;
            _productService = productService;
            _pricingService = pricingService;
            _settingsManager = settingsManager;
        }

        public ReadinessEntry[] EvaluateReadiness(ReadinessChannel channel, CatalogProduct[] products)
        {
            var entries = new ReadinessEntry[products.Length];
            var pricelist = _pricingService.GetPricelistsById(new[] { channel.PricelistId }).FirstOrDefault();
            if (pricelist == null)
            {
                throw new NullReferenceException("pricelist");
            }
            for (var i = 0; i < products.Length; i++)
            {
                var product = products[i];
                if (product.Properties.IsNullOrEmpty() || product.Reviews.IsNullOrEmpty() || product.SeoInfos.IsNullOrEmpty())
                {
                    product = _productService.GetById(product.Id, ItemResponseGroup.ItemProperties | ItemResponseGroup.ItemEditorialReviews | ItemResponseGroup.Seo, channel.CatalogId);
                }
                var entry = new ReadinessEntry
                {
                    ChannelId = channel.Id,
                    ProductId = product.Id,
                    Details = new[]
                    {
                        ValidateProperties(channel, product),
                        ValidateDescriptions(channel, product),
                        ValidatePrices(pricelist, product),
                        ValidateSeo(channel, product),
                    }
                };
                entry.ReadinessPercent = (int) Math.Round((double) entry.Details.Sum(x => x.ReadinessPercent) / entry.Details.Length);
                entries[i] = entry;
            }
            _readinessService.SaveEntries(entries);
            return entries;
        }

        private ReadinessDetail ValidateProperties(ReadinessChannel channel, CatalogProduct product)
        {
            var retVal = new ReadinessDetail { Name = "Properties" };
            var properties = product.Properties.Where(x => x.Required).ToArray();
            var invalidProperties = properties.Where(p =>
            {
                var values = product.PropertyValues.Where(x => x.Property.Id == p.Id).ToArray();
                return values.IsNullOrEmpty() || p.Dictionary
                    ? values.All(x => !p.DictionaryValues.Any(y => y.LanguageCode == channel.Language && y.Value == x.Value.ToString()))
                    : values.All(x => x.LanguageCode != channel.Language && x.Value != null &&
                                        (x.ValueType != PropertyValueType.ShortText && x.ValueType != PropertyValueType.LongText || !string.IsNullOrEmpty((string) x.Value)) &&
                                        (x.ValueType != PropertyValueType.Number || (decimal) x.Value >= 0m));
            });
            retVal.ReadinessPercent = CalculateReadiness(properties.Length, invalidProperties.Count());
            return retVal;
        }

        private ReadinessDetail ValidateDescriptions(ReadinessChannel channel, CatalogProduct product)
        {
            var retVal = new ReadinessDetail { Name = "Descriptions" };
            var descriptionTypes = _settingsManager.GetSettingByName("Catalog.EditorialReviewTypes").AllowedValues;
            var missedDescriptionTypes = descriptionTypes.Except(product.Reviews
                .Where(x => x.LanguageCode == channel.Language && !string.IsNullOrEmpty(x.Content))
                .Select(x => x.ReviewType)
                .Distinct());
            retVal.ReadinessPercent = CalculateReadiness(descriptionTypes.Length, missedDescriptionTypes.Count());
            return retVal;
        }

        private ReadinessDetail ValidatePrices(Pricelist pricelist, CatalogProduct product)
        {
            var retVal = new ReadinessDetail
            {
                Name = "Prices",
                ReadinessPercent = pricelist.Prices.Any(x => x.ProductId == product.Id && x.List > 0) ? 100 : 0
            };
            return retVal;
        }

        private ReadinessDetail ValidateSeo(ReadinessChannel channel, CatalogProduct product)
        {
            var retVal = new ReadinessDetail { Name = "Seo" };
            string pattern = @"[$+;=%{}[\]|\\\/@ ~#!^*&?:'<>,]";
            retVal.ReadinessPercent = product.SeoInfos.Any(x => x.LanguageCode == channel.Language && !Regex.IsMatch(x.SemanticUrl, pattern)) ? 100 : 0;
            return retVal;
        }

        private int CalculateReadiness(int validCount, int invalidCount)
        {
            return validCount == 0 || invalidCount == 0 ? 100 : (int) Math.Round((double) validCount / (double) invalidCount) * 100;
        }
    }
}