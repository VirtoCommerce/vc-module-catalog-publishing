using System;
using System.Globalization;
using System.Linq;
using VirtoCommerce.CatalogPublishingModule.Core.Model;
using VirtoCommerce.CatalogPublishingModule.Data.Core.Common;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogPublishingModule.Data.Core.Services
{
    public class PropertiesReadinessDetailEvaluator : DefaultReadinessDetailEvaluator
    {
        public override ReadinessDetail[] EvaluateReadiness(ReadinessChannel channel, CatalogProduct[] products)
        {
            return products.Select(x =>
            {
                var detail = new ReadinessDetail { Name = "Properties", ProductId = x.Id };
                if (x.Properties.IsNullOrEmpty())
                {
                    detail.ReadinessPercent = 100;
                }
                else
                {
                    var properties = x.Properties.Where(p => p != null && p.Required).ToArray();
                    var singleLanguageProperties = properties.Where(p => !p.Multilanguage).ToArray();
                    var multiLanguageProperties = properties.Where(p => p.Multilanguage).ToArray();
                    var invalidPropertiesCount = singleLanguageProperties.Where(p =>
                        {
                            var values = x.PropertyValues.Where(v => v.Property != null && v.Property.Id == p.Id).ToArray();
                            return IsInvalidProperty(p, values);
                        })
                        .Count();
                    var invalidPropertiesPerLanguageCount = channel.Languages
                        .Select(l => multiLanguageProperties
                            .Where(p =>
                            {
                                var values = x.PropertyValues.Where(v => v.Property != null && v.Property.Id == p.Id).ToArray();
                                return IsInvalidProperty(p, values, l);
                            })
                            .Count())
                        .Sum();
                    detail.ReadinessPercent = ReadinessHelper.CalculateReadiness(singleLanguageProperties.Length + multiLanguageProperties.Length * channel.Languages.Count,
                        invalidPropertiesCount + invalidPropertiesPerLanguageCount);
                }
                return detail;
            }).ToArray();
        }

        private static bool IsInvalidProperty(Property property, PropertyValue[] values, string languageCode = null)
        {
            var retVal = values.IsNullOrEmpty();
            if (!retVal)
            {
                if (property.Dictionary)
                {
                    retVal = values.All(pv => !property.DictionaryValues.IsNullOrEmpty() &&
                                             (property.Multilanguage && pv.LanguageCode != languageCode || !property.DictionaryValues.Any(dv => IsEqualValues(pv.ValueType, dv.Value, pv.Value))));
                }
                else
                {
                    retVal = values.All(pv => property.Multilanguage && pv.LanguageCode != languageCode || IsInvalidPropertyValue(pv.ValueType, pv.Value));
                }
            }
            return retVal;
        }

        private static bool IsEqualValues(PropertyValueType type, string first, object second)
        {
            var retVal = false;
            bool successfulParse;
            switch (type)
            {
                case PropertyValueType.ShortText:
                case PropertyValueType.LongText:
                    retVal = first == (string) second;
                    break;
                case PropertyValueType.Number:
                    decimal parsedDecimal;
                    successfulParse = decimal.TryParse(first.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out parsedDecimal);
                    retVal = successfulParse && parsedDecimal == (decimal) second;
                    break;
                case PropertyValueType.DateTime:
                    DateTime parsedDateTime;
                    successfulParse = DateTime.TryParse(first, out parsedDateTime);
                    retVal = successfulParse && parsedDateTime == (DateTime) second;
                    break;
                case PropertyValueType.Boolean:
                    bool parsedBool;
                    successfulParse = bool.TryParse(first, out parsedBool);
                    retVal = successfulParse && parsedBool == (bool) second;
                    break;
            }
            return retVal;
        }
        
        private static bool IsInvalidPropertyValue(PropertyValueType type, object value)
        {
            var retVal = value == null;
            if (!retVal)
            {
                switch (type)
                {
                    case PropertyValueType.ShortText:
                    case PropertyValueType.LongText:
                        retVal = string.IsNullOrEmpty((string)value);
                        break;
                    case PropertyValueType.Number:
                        retVal = (decimal)value < 0m;
                        break;
                    // No checks for DateTime & Boolean - any value is valid
                }
            }
            return retVal;
        }
    }
}