using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogPublishingModule.Core.Model.Search
{
    public class CompletenessChannelSearchResult : GenericSearchResult<CompletenessChannel>
    {
        public string[] CatalogIds { get; set; }
    }
}
