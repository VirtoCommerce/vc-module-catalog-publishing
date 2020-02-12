using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogPublishingModule.Core.Model;

namespace VirtoCommerce.CatalogPublishingModule.Core.Services
{
    /// <summary>
    /// ICompletenessEvaluator is a base interface for all completeness evaluators. Implement &amp; register implementation to customize evaluation process
    /// </summary>
    public interface ICompletenessEvaluator
    {
        CompletenessEntry[] EvaluateCompleteness(CompletenessChannel channel, CatalogProduct[] products);
    }
}
