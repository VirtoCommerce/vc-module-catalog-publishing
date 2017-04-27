using VirtoCommerce.Domain.Commerce.Model.Search;

namespace VirtoCommerce.CatalogPublishingModule.Core.Model.Search
{
    public class ReadinessChannelSearchCriteria : SearchCriteriaBase
    {
        public string[] CatalogIds { get; set; }
    }
}