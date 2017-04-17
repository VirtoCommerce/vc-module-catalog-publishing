using VirtoCommerce.CatalogPublishingModule.Core.Model;
using VirtoCommerce.CatalogPublishingModule.Core.Model.Search;
using VirtoCommerce.Domain.Commerce.Model.Search;

namespace VirtoCommerce.CatalogPublishingModule.Core.Services
{
    public interface IReadinessService
    {
        ReadinessChannel[] GetChannelsByIds(string[] ids);

        void SaveEntries(ReadinessEntry[] entries);

        void SaveChannels(ReadinessChannel[] channels);

        GenericSearchResult<ReadinessChannel> SearchChannels(ReadinessChannelSearchCriteria criteria);

        void DeleteChannels(string[] ids);
    }
}