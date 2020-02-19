namespace VirtoCommerce.CatalogPublishingModule.Test.Model
{
    public class EditorialReview : CatalogModule.Core.Model.EditorialReview
    {
        public override string ToString()
        {
            return $"{{ LanguageCode: {LanguageCode}, Content: {Content}, ReviewType: {ReviewType} }}";
        }
    }
}
