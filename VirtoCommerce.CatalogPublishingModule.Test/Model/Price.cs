namespace VirtoCommerce.CatalogPublishingModule.Test.Model
{
    public class Price : Domain.Pricing.Model.Price
    {
        public override string ToString()
        {
            return string.Format("{{ ProductId: {0}, PricelistId: {1}, List: {2} }}", ProductId, PricelistId, List);
        }
    }
}