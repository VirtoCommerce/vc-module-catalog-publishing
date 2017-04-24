using System.Linq;

namespace VirtoCommerce.CatalogPublishingModule.Test.Model
{
    public class Property : Domain.Catalog.Model.Property
    {
        public override string ToString()
        {
            return string.Format("{{ Id: {0}, Required: {1}, Dictionary: {2}, DicrionaryValues: [{3}], ValueType: {4} }}",
                Id,
                Required,
                Dictionary,
                DictionaryValues != null ? string.Join(", ", DictionaryValues.Select(x => x.ToString())) : null,
                ValueType);
        }
    }
}