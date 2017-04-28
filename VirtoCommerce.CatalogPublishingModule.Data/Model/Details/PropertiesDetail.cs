using System;
using System.Globalization;
using System.Linq;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Pricing.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogPublishingModule.Data.Model.Details
{
    public class PropertiesDetail : DefaultReadinessDetail
    {
        public PropertiesDetail()
        {
            Name = "Properties";
        }

        public override void Evaluate(CatalogProduct product, string pricelistId, Price[] productPrices, string language)
        {
            int readinessPercent;
            if (product.Properties.IsNullOrEmpty())
            {
                readinessPercent = 100;
            }
            else
            {
                var properties = product.Properties.Where(x => x != null && x.Required).ToArray();
                var invalidProperties = properties.Where(p =>
                {
                    var values = product.PropertyValues.Where(x => x.Property != null && x.Property.Id == p.Id).ToArray();
                    return IsInvalidProperty(p, values, language);
                });
                readinessPercent = ReadinessHelper.CalculateReadiness(properties.Length, invalidProperties.Count());
            }
            ReadinessPercent = readinessPercent;
        }

        private bool IsInvalidProperty(Property property, PropertyValue[] values, string languageCode)
        {
            var retVal = values.IsNullOrEmpty();
            if (!retVal)
            {
                if (property.Dictionary)
                {
                    retVal = values.Any(x => !property.DictionaryValues.IsNullOrEmpty() && (x.LanguageCode != languageCode ||
                                                                                            !property.DictionaryValues.Any(y => IsEqualValues(x.ValueType, y.Value, x.Value))));
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