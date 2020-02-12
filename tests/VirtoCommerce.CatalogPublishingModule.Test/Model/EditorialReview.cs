namespace VirtoCommerce.CatalogPublishingModule.Test.Model
{
    public class EditorialReview : Domain.Catalog.Model.EditorialReview
    {
        public override string ToString()
        {
            return $"{{ LanguageCode: {LanguageCode}, Content: {Content}, ReviewType: {ReviewType} }}";
        }
    }
}