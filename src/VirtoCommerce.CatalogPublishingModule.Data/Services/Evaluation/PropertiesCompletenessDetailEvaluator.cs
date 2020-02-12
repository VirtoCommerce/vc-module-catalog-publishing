using System;
using System.Globalization;
using System.Linq;
using VirtoCommerce.CatalogPublishingModule.Core.Model;
using VirtoCommerce.CatalogPublishingModule.Core.Services;
using VirtoCommerce.CatalogPublishingModule.Data.Common;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogPublishingModule.Data.Services.Evaluation
{
    /// <summary>
    /// Properties validator. Check the following statements:
    /// • One of property dictionary values (for dictionary properties);
    /// • For value of type Short text & Long text: is not null or empty;
    /// • For value of type Number: is greater than or equal to zero;
    /// • Any value successfully parsed as Date time or Boolean is valid;
    /// </summary>
    public class PropertiesCompletenessDetailEvaluator : ICompletenessDetailEvaluator
    {
        public CompletenessDetail[] EvaluateCompleteness(CompletenessChannel channel, CatalogProduct[] products)
        {
            return products.Select(x =>
            {
                var detail = new CompletenessDetail { Name = "Properties", ProductId = x.Id };
                if (x.Properties.IsNullOrEmpty())
                {
                    detail.CompletenessPercent = 100;
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
                    detail.CompletenessPercent = CompletenessHelper.CalculateCompleteness(singleLanguageProperties.Length + multiLanguageProperties.Length * channel.Languages.Count,
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
                    retVal = values.All(pv => !property.DictionaryValues.IsNullOrEmpty() && (property.ValueType != PropertyValueType.ShortText ||
                                             property.Multilanguage && pv.LanguageCode != languageCode || property.DictionaryValues.All(dv => dv.Value != (string) pv.Value)));
                }
                else
                {
                    retVal = values.All(pv => property.Multilanguage && pv.LanguageCode != languageCode || IsInvalidPropertyValue(pv.ValueType, pv.Value));
                }
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