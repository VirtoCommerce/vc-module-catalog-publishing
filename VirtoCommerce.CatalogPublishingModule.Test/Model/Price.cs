namespace VirtoCommerce.CatalogPublishingModule.Test.Model
{
    public class Price : Domain.Pricing.Model.Price
    {
        public override string ToString()
        {
            return $"{{ ProductId: {ProductId}, PricelistId: {PricelistId}, List: {List} }}";
        }
    }
}