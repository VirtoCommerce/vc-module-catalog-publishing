namespace VirtoCommerce.CatalogPublishingModule.Test.Model
{
    public class Price : PricingModule.Core.Model.Price
    {
        public override string ToString()
        {
            return $"{{ ProductId: {ProductId}, PricelistId: {PricelistId}, List: {List} }}";
        }
    }
}
