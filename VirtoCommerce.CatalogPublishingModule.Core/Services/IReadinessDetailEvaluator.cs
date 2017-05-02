using VirtoCommerce.CatalogPublishingModule.Core.Model;
using VirtoCommerce.Domain.Catalog.Model;

namespace VirtoCommerce.CatalogPublishingModule.Core.Services
{
    public interface IReadinessDetailEvaluator
    {
        ReadinessDetail[] EvaluateReadiness(ReadinessChannel channel, CatalogProduct[] products);
    }
}