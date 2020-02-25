using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogPublishingModule.Core.Model;

namespace VirtoCommerce.CatalogPublishingModule.Core.Services
{
    /// <summary>
    /// ICompletenessDetailEvaluator is a base interface for all detail evaluators. Implement &amp; register implementaton to provide your own validator for product completeness
    /// </summary>
    public interface ICompletenessDetailEvaluator
    {
        Task<CompletenessDetail[]> EvaluateCompletenessAsync(CompletenessChannel channel, CatalogProduct[] products);
    }
}
