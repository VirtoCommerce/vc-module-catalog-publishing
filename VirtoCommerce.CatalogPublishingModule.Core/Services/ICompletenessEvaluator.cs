using VirtoCommerce.CatalogPublishingModule.Core.Model;
using VirtoCommerce.Domain.Catalog.Model;

namespace VirtoCommerce.CatalogPublishingModule.Core.Services
{
    /// <summary>
    /// ICompletenessEvaluator is a base interface for all completeness evaluators. Implement & register implementation to customize evaluation process
    /// </summary>
    public interface ICompletenessEvaluator
    {
        CompletenessEntry[] EvaluateCompleteness(CompletenessChannel channel, CatalogProduct[] products);
    }
}