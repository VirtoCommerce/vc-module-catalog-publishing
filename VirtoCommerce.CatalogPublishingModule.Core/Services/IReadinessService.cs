using VirtoCommerce.CatalogPublishingModule.Core.Model;
using VirtoCommerce.CatalogPublishingModule.Core.Model.Search;
using VirtoCommerce.Domain.Commerce.Model.Search;

namespace VirtoCommerce.CatalogPublishingModule.Core.Services
{
    public interface IReadinessService
    {
        ReadinessChannel[] GetChannelsByIds(string[] ids);

        ReadinessEntry[] GetReadinessEntriesByIds(string[] ids);

        void SaveChannels(ReadinessChannel[] channels);

        void SaveEntries(ReadinessEntry[] entries);

        GenericSearchResult<ReadinessChannel> SearchChannels(ReadinessChannelSearchCriteria criteria);

        void DeleteChannels(string[] ids);
    }
}