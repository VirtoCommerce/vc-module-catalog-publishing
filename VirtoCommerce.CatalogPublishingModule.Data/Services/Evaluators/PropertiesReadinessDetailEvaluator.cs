using System;
using System.Globalization;
using System.Linq;
using VirtoCommerce.CatalogPublishingModule.Core.Model;
using VirtoCommerce.CatalogPublishingModule.Data.Common;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogPublishingModule.Data.Services.Evaluators
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
                    var invalidProperties = properties.Where(p =>
                    {
                        var values = x.PropertyValues.Where(v => v.Property != null && v.Property.Id == p.Id).ToArray();
                        return IsInvalidProperty(p, values, channel.Language);
                    });
                    detail.ReadinessPercent = ReadinessHelper.CalculateReadiness(properties.Length, invalidProperties.Count());
                }
                return detail;
            }).ToArray();
        }

        private static bool IsInvalidProperty(Property property, PropertyValue[] values, string languageCode)
        {
            var retVal = values.IsNullOrEmpty();
            if (!retVal)
            {
                if (property.Dictionary)
                {
                    retVal = values.Any(x => !property.DictionaryValues.IsNullOrEmpty() &&
                                             (x.LanguageCode != languageCode || !property.DictionaryValues.Any(y => IsEqualValues(x.ValueType, y.Value, x.Value))));
                }
                else
                {
                    retVal = values.Any(x => x.LanguageCode != languageCode || IsInvalidPropertyValue(x.ValueType, x.Value));
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