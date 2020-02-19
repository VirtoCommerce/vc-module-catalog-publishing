namespace VirtoCommerce.CatalogPublishingModule.Test.Model
{
    public class Property : CatalogModule.Core.Model.Property
    {
        public override string ToString()
        {
            return $"{{ Id: {Id}, Required: {Required}, Multilanguage: {Multilanguage} Dictionary: {Dictionary}, ValueType: {ValueType} }}";
        }
    }
}
