using VirtoCommerce.CatalogPublishingModule.Core.Model;
using VirtoCommerce.Domain.Catalog.Model;

namespace VirtoCommerce.CatalogPublishingModule.Core.Services
{
    /// <summary>
    /// ICompletenessDetailEvaluator is a base interface for all detail evaluators. Implement & register implementaton to provide your own validator for product completeness
    /// </summary>
    public interface ICompletenessDetailEvaluator
    {
        CompletenessDetail[] EvaluateCompleteness(CompletenessChannel channel, CatalogProduct[] products);
    }
}