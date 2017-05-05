namespace VirtoCommerce.CatalogPublishingModule.Test.Model
{
    public class PropertyDictionaryValue : Domain.Catalog.Model.PropertyDictionaryValue
    {
        public override string ToString()
        {
            return $"{{ LanguageCode: {LanguageCode}, Value: {Value} }}";
        }
    }
}