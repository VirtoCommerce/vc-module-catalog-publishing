namespace VirtoCommerce.CatalogPublishingModule.Test.Model
{
    public class PropertyValue : Domain.Catalog.Model.PropertyValue
    {
        public override string ToString()
        {
            return $"{{ Property: {Property.Id}, LanguageCode: {LanguageCode}, ValueType: {ValueType}, Value: {Value} }}";
        }
    }
}