using System.Linq;

namespace VirtoCommerce.CatalogPublishingModule.Test.Model
{
    public class Property : Domain.Catalog.Model.Property
    {
        public override string ToString()
        {
            return $"{{ Id: {Id}, Required: {Required}, Dictionary: {Dictionary}, " +
                   $"DicrionaryValues: [{(DictionaryValues != null ? string.Join(", ", DictionaryValues.Select(x => x.ToString())) : null)}], ValueType: {ValueType} }}";
        }
    }
}