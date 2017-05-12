using VirtoCommerce.Domain.Commerce.Model.Search;

namespace VirtoCommerce.CatalogPublishingModule.Core.Model.Search
{
    public class CompletenessChannelSearchCriteria : SearchCriteriaBase
    {
        public string[] CatalogIds { get; set; }
    }
}