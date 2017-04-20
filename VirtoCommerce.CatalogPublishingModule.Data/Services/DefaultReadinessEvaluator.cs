﻿using System;
using System.Globalization;
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

                UpdateReadinessProperty(channel.Name, product, entry.ReadinessPercent);
            }
            _readinessService.SaveEntries(entries);
            _productService.Update(products);
            return entries;
        }

        private void UpdateReadinessProperty(string channelName, CatalogProduct product, int readinessPercent)
        {
            var readinessPropertyName = "readiness_" + channelName;
                var readinessProperty = product.Properties.FirstOrDefault(x => x.Name == readinessPropertyName);
                if (readinessProperty == null)
                {
                    readinessProperty = new Property
                    {
                        Name = readinessPropertyName,
                        ValueType = PropertyValueType.Number
                    };
                    product.Properties.Add(readinessProperty);
                }
                var readinessPropertyValue = product.PropertyValues.FirstOrDefault(x => x.PropertyName == readinessPropertyName);
                if (readinessPropertyValue == null)
                {
                    readinessPropertyValue = new PropertyValue();
                    product.PropertyValues.Add(readinessPropertyValue);
                }
                readinessPropertyValue.Value = readinessPercent;
        }

        private ReadinessDetail ValidateProperties(ReadinessChannel channel, CatalogProduct product)
        {
            var retVal = new ReadinessDetail { Name = "Properties" };
            var properties = product.Properties.Where(x => x.Required).ToArray();
            var invalidProperties = properties.Where(p =>
            {
                var values = product.PropertyValues.Where(x => x.Property.Id == p.Id).ToArray();
                return IsInvalidProperty(p, values, channel.Language);
            });
            retVal.ReadinessPercent = CalculateReadiness(properties.Length, invalidProperties.Count());
            return retVal;
        }

        private bool IsInvalidProperty(Property property, PropertyValue[] values, string languageCode)
        {
            var retVal = values.IsNullOrEmpty();
            if (!retVal)
            {
                if (property.Dictionary)
                {
                    retVal = values.All(x => !property.DictionaryValues.Any(y => y.LanguageCode == languageCode && IsEqualValues(x.ValueType, y.Value, x.Value)));
                }
                else
                {
                    retVal = values.All(x => x.LanguageCode != languageCode && IsValidPropertyValue(x.ValueType, x.Value));
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