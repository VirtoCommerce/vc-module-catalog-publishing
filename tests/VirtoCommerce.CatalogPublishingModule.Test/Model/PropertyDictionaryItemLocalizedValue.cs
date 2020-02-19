namespace VirtoCommerce.CatalogPublishingModule.Test.Model
{
    public class PropertyDictionaryItemLocalizedValue : CatalogModule.Core.Model.PropertyDictionaryItemLocalizedValue
    {
        public override string ToString()
        {
            return $"{{ LanguageCode: {LanguageCode}, Value: {Value} }}";
        }
    }
}
