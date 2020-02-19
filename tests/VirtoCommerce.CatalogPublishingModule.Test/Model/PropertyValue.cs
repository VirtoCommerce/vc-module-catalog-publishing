namespace VirtoCommerce.CatalogPublishingModule.Test.Model
{
    public class PropertyValue : CatalogModule.Core.Model.PropertyValue
    {
        public override string ToString()
        {
            return $"{{ Property: {Property.Id}, LanguageCode: {LanguageCode}, ValueType: {ValueType}, Value: {Value} }}";
        }
    }
}
