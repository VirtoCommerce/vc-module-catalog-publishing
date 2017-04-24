namespace VirtoCommerce.CatalogPublishingModule.Test.Model
{
    public class PropertyValue : Domain.Catalog.Model.PropertyValue
    {
        public override string ToString()
        {
            return string.Format("{{ LanguageCode: {0}, ValueType: {1}, Value: {2} }}", LanguageCode, ValueType, Value);
        }
    }
}