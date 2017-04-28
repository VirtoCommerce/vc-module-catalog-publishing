using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using VirtoCommerce.CatalogPublishingModule.Core.Model;
using VirtoCommerce.CatalogPublishingModule.Core.Services;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Domain.Pricing.Model;
using VirtoCommerce.Domain.Pricing.Model.Search;
using VirtoCommerce.Domain.Pricing.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.CatalogPublishingModule.Data.Services
{
    public class DefaultReadinessEvaluator : IReadinessEvaluator
    {
        private readonly IReadinessService _readinessService;
        private readonly IItemService _productService;
        private readonly IPricingSearchService _pricingSearchService;
        private readonly ISettingsManager _settingsManager;

        public DefaultReadinessEvaluator(IReadinessService readinessService, IItemService productService, IPricingSearchService pricingSearchService, ISettingsManager settingsManager)
        {
            _readinessService = readinessService;
            _productService = productService;
            _pricingSearchService = pricingSearchService;
            _settingsManager = settingsManager;
        }

        public ReadinessEntry[] EvaluateReadiness(ReadinessChannel channel, CatalogProduct[] products)
        {
            if (channel == null)
            {
                throw new ArgumentNullException("channel");
            }
            if (products.IsNullOrEmpty())
            {
                throw new ArgumentException("Products must be specified", "products");
            }
            var prices = _pricingSearchService.SearchPrices(new PricesSearchCriteria
            {
                PriceListId = channel.PricelistId, ProductIds = products.Select(x => x.Id).ToArray(), Take = int.MaxValue
            }).Results;

            products = _productService.GetByIds(products.Select(x => x.Id).ToArray(), ItemResponseGroup.ItemLarge);
            products = products.Where(x => x.Outlines.Any(o => o.Items.FirstOrDefault()?.Id == channel.CatalogId )).ToArray();

            var entries = new ReadinessEntry[products.Length];

            for (var i = 0; i < products.Length; i++)
            {
                var product = products[i];

                var entry = new ReadinessEntry
                {
                    ChannelId = channel.Id,
                    ProductId = product.Id,
                    Details = new[]
                    {
                        ValidateProperties(channel, product),
                        ValidateDescriptions(channel, product),
                        ValidatePrices(prices?.Where(x => x.ProductId == product.Id)),
                        ValidateSeo(channel, product),
                    }
                };
                entry.ReadinessPercent = (int) Math.Round(entry.Details.Average(x => x.ReadinessPercent));
                entries[i] = entry;
            }

            _readinessService.SaveEntries(entries);

            return entries;
        }

        private ReadinessDetail ValidateProperties(ReadinessChannel channel, CatalogProduct product)
        {
            var retVal = new ReadinessDetail { Name = "Properties" };
            if (product.Properties.IsNullOrEmpty())
            {
                retVal.ReadinessPercent = 100;
            }
            else
            {
                var properties = product.Properties.Where(x => x != null && x.Required).ToArray();
                var invalidProperties = properties.Where(p =>
                {
                    var values = product.PropertyValues.Where(x => x.Property != null && x.Property.Id == p.Id).ToArray();
                    return IsInvalidProperty(p, values, channel.Language);
                });
                retVal.ReadinessPercent = CalculateReadiness(properties.Length, invalidProperties.Count());
            }
            return retVal;
        }

        private bool IsInvalidProperty(Property property, PropertyValue[] values, string languageCode)
        {
            var retVal = values.IsNullOrEmpty();
            if (!retVal)
            {
                if (property.Dictionary)
                {
                    retVal = values.Any(x => !property.DictionaryValues.IsNullOrEmpty() && (x.LanguageCode != languageCode || !property.DictionaryValues.Any(y => IsEqualValues(x.ValueType, y.Value, x.Value))));
                }
                else
                {
                    retVal = values.Any(x => x.LanguageCode != languageCode || !IsValidPropertyValue(x.ValueType, x.Value));
                }
            }
            return retVal;
        }

        private bool IsEqualValues(PropertyValueType type, string first, object second)
        {
            var retVal = false;
            bool successfulParse;
            switch (type)
            {
                case PropertyValueType.ShortText:
                case PropertyValueType.LongText:
                    retVal = first == (string)second;
                    break;
                case PropertyValueType.Number:
                    decimal parsedDecimal;
                    successfulParse = decimal.TryParse(first.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out parsedDecimal);
                    retVal = successfulParse && parsedDecimal == (decimal)second;
                    break;
                case PropertyValueType.DateTime:
                    DateTime parsedDateTime;
                    successfulParse = DateTime.TryParse(first, out parsedDateTime);
                    retVal = successfulParse && parsedDateTime == (DateTime)second;
                    break;
                case PropertyValueType.Boolean:
                    bool parsedBool;
                    successfulParse = bool.TryParse(first, out parsedBool);
                    retVal = successfulParse && parsedBool == (bool)second;
                    break;
            }
            return retVal;
        }

        private bool IsValidPropertyValue(PropertyValueType type, object value)
        {
            var retVal = value != null;
            if (retVal)
            {
                switch (type)
                {
                    case PropertyValueType.ShortText:
                    case PropertyValueType.LongText:
                        retVal = !string.IsNullOrEmpty((string) value);
                        break;
                    case PropertyValueType.Number:
                        retVal = (decimal) value >= 0m;
                        break;
                    // No checks for DateTime & Boolean - any value is valid
                }
            }
            return retVal;
        }

        private ReadinessDetail ValidateDescriptions(ReadinessChannel channel, CatalogProduct product)
        {
            var retVal = new ReadinessDetail { Name = "Descriptions" };
            var descriptionTypes = _settingsManager.GetSettingByName("Catalog.EditorialReviewTypes").ArrayValues;
            if (descriptionTypes.IsNullOrEmpty())
            {
                retVal.ReadinessPercent = 100;
            }
            else
            {
                var missedDescriptionTypes = descriptionTypes.Except(product.Reviews
                    .Where(x => x.LanguageCode == channel.Language && !string.IsNullOrEmpty(x.Content))
                    .Select(x => x.ReviewType)
                    .Distinct());
                retVal.ReadinessPercent = CalculateReadiness(descriptionTypes.Length, missedDescriptionTypes.Count());
            }
            return retVal;
        }

        private ReadinessDetail ValidatePrices(IEnumerable<Price> prices)
        {
            var retVal = new ReadinessDetail
            {
                Name = "Prices",
                ReadinessPercent = prices != null && prices.Any(x => x.List > 0) ? 100 : 0
            };
            return retVal;
        }

        private ReadinessDetail ValidateSeo(ReadinessChannel channel, CatalogProduct product)
        {
            var retVal = new ReadinessDetail { Name = "Seo" };
            string pattern = @"[$+;=%{}[\]|\\\/@ ~#!^*&?:'<>,]";
            retVal.ReadinessPercent = product.SeoInfos.Any(x => x.LanguageCode == channel.Language && !string.IsNullOrEmpty(x.SemanticUrl) && !Regex.IsMatch(x.SemanticUrl, pattern)) ? 100 : 0;
            return retVal;
        }

        private int CalculateReadiness(int totalCount, int invalidCount)
        {
            int retVal;
            if (totalCount == 0 || invalidCount == 0)
            {
                retVal = 100;
            }
            else if (totalCount == invalidCount)
            {
                retVal = 0;
            }
            else
            {
                retVal = (int) Math.Round((totalCount - invalidCount) / (double) totalCount * 100);
            }
            return retVal;
        }
    }
}