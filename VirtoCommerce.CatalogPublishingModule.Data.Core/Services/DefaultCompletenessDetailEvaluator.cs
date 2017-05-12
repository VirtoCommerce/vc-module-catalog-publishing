using VirtoCommerce.CatalogPublishingModule.Core.Model;
using VirtoCommerce.CatalogPublishingModule.Core.Services;
using VirtoCommerce.Domain.Catalog.Model;

namespace VirtoCommerce.CatalogPublishingModule.Data.Core.Services
{
    public abstract class DefaultCompletenessDetailEvaluator : ICompletenessDetailEvaluator
    {
        public abstract CompletenessDetail[] EvaluateCompleteness(CompletenessChannel channel, CatalogProduct[] products);
    }
}