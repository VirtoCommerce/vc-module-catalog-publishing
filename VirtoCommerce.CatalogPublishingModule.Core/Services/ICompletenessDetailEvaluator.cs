using VirtoCommerce.CatalogPublishingModule.Core.Model;
using VirtoCommerce.Domain.Catalog.Model;

namespace VirtoCommerce.CatalogPublishingModule.Core.Services
{
    public interface ICompletenessDetailEvaluator
    {
        CompletenessDetail[] EvaluateCompleteness(CompletenessChannel channel, CatalogProduct[] products);
    }
}