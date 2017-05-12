using VirtoCommerce.CatalogPublishingModule.Core.Model;
using VirtoCommerce.CatalogPublishingModule.Core.Model.Search;
using VirtoCommerce.Domain.Commerce.Model.Search;

namespace VirtoCommerce.CatalogPublishingModule.Core.Services
{
    public interface ICompletenessService
    {
        CompletenessChannel[] GetChannelsByIds(string[] ids);

        CompletenessEntry[] GetCompletenessEntriesByIds(string[] ids);

        void SaveChannels(CompletenessChannel[] channels);

        void SaveEntries(CompletenessEntry[] entries);

        GenericSearchResult<CompletenessChannel> SearchChannels(CompletenessChannelSearchCriteria criteria);

        void DeleteChannels(string[] ids);
    }
}