using VirtoCommerce.CatalogPublishingModule.Core.Model;
using VirtoCommerce.CatalogPublishingModule.Core.Services;
using VirtoCommerce.Domain.Catalog.Model;

namespace VirtoCommerce.CatalogPublishingModule.Data.Services
{
    public abstract class DefaultReadinessDetailEvaluator : IReadinessDetailEvaluator
    {
        public abstract ReadinessDetail[] EvaluateReadiness(ReadinessChannel channel, CatalogProduct[] products);
    }
}