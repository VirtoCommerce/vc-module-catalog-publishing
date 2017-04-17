using VirtoCommerce.CatalogPublishingModule.Core.Model;
using VirtoCommerce.Domain.Catalog.Model;

namespace VirtoCommerce.CatalogPublishingModule.Core.Services
{
    public interface IReadinessEvaluator
    {
        ReadinessEntry[] EvaluateReadiness(ReadinessChannel channel, CatalogProduct[] products);
    }
}