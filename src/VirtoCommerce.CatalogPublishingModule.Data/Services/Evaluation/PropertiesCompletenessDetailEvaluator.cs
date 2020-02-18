using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogPublishingModule.Core.Model;
using VirtoCommerce.CatalogPublishingModule.Core.Services;
using VirtoCommerce.CatalogPublishingModule.Data.Common;
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
        private readonly IPropertyDictionaryItemSearchService _propertyDictionaryItemSearchService;
        private readonly Dictionary<string, bool> _propertyDictionaryHasItemsCache = new Dictionary<string, bool>();

        public PropertiesCompletenessDetailEvaluator(IPropertyDictionaryItemSearchService propertyDictionaryItemSearchService)
        {
            _propertyDictionaryItemSearchService = propertyDictionaryItemSearchService;
        }

        public async Task<CompletenessDetail[]> EvaluateCompletenessAsync(CompletenessChannel channel, CatalogProduct[] products)
        {
            var result = new List<CompletenessDetail>();
            foreach (var product in products)
            {
                var detail = new CompletenessDetail { Name = "Properties", ProductId = product.Id };
                if (product.Properties.IsNullOrEmpty())
                {
                    detail.CompletenessPercent = 100;
                }
                else
                {
                    var properties = product.Properties.Where(p => p != null && p.Required).ToArray();
                    foreach (var property in properties)
                    {
                        await CountPropertyDictionaryItems(property.Id);
                    }
                    var singleLanguageProperties = properties.Where(p => !p.Multilanguage).ToArray();
                    var multiLanguageProperties = properties.Where(p => p.Multilanguage).ToArray();
                    var invalidPropertiesCount = singleLanguageProperties.Count(p =>
                    {
                        var values = p.Values.ToArray();
                        return IsInvalidProperty(p, values);
                    });
                    var invalidPropertiesPerLanguageCount = channel.Languages
                        .Select(l => multiLanguageProperties
                            .Count(p =>
                            {
                                var values = p.Values.ToArray();
                                return IsInvalidProperty(p, values, l);
                            }))
                        .Sum();
                    detail.CompletenessPercent = CompletenessHelper.CalculateCompleteness(singleLanguageProperties.Length + multiLanguageProperties.Length * channel.Languages.Count,
                        invalidPropertiesCount + invalidPropertiesPerLanguageCount);
                }
                result.Add(detail);
            }
            return result.ToArray();
        }

        private bool IsInvalidProperty(Property property, PropertyValue[] values, string languageCode = null)
        {
            var result = values.IsNullOrEmpty();
            if (!result)
            {
                if (property.Dictionary)
                {
                    var dictionaryHasItems = _propertyDictionaryHasItemsCache[property.Id];
                    result = values.All(pv => dictionaryHasItems && (property.ValueType != PropertyValueType.ShortText || property.Multilanguage && pv.LanguageCode != languageCode)
                    );
                }
                else
                {
                    result = values.All(pv => property.Multilanguage && pv.LanguageCode != languageCode || IsInvalidPropertyValue(pv.ValueType, pv.Value));
                }
            }
            return result;
        }

        private async Task CountPropertyDictionaryItems(string propertyId)
        {
            if (!_propertyDictionaryHasItemsCache.ContainsKey(propertyId))
            {
                var items = await _propertyDictionaryItemSearchService.SearchAsync(
                    new PropertyDictionaryItemSearchCriteria
                    {
                        PropertyIds = new[] { propertyId },
                        Take = 0,
                    });
                _propertyDictionaryHasItemsCache.Add(propertyId, items.TotalCount > 0);
            }
        }

        private static bool IsInvalidPropertyValue(PropertyValueType type, object value)
        {
            var result = value == null;
            if (!result)
            {
                switch (type)
                {
                    case PropertyValueType.ShortText:
                    case PropertyValueType.LongText:
                        result = string.IsNullOrEmpty((string)value);
                        break;
                    case PropertyValueType.Number:
                        result = (decimal)value < 0m;
                        break;
                        // No checks for DateTime & Boolean - any value is valid
                }
            }
            return result;
        }
    }
}
