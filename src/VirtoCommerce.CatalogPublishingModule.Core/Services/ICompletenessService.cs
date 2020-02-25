using System.Threading.Tasks;
using VirtoCommerce.CatalogPublishingModule.Core.Model;
using VirtoCommerce.CatalogPublishingModule.Core.Model.Search;

namespace VirtoCommerce.CatalogPublishingModule.Core.Services
{
    public interface ICompletenessService
    {
        Task<CompletenessChannel[]> GetChannelsByIdsAsync(string[] ids);

        Task<CompletenessEntry[]> GetCompletenessEntriesByIdsAsync(string[] ids);

        Task SaveChannelsAsync(CompletenessChannel[] channels);

        Task SaveEntriesAsync(CompletenessEntry[] entries);

        Task<CompletenessChannelSearchResult> SearchChannelsAsync(CompletenessChannelSearchCriteria criteria);

        Task DeleteChannelsAsync(string[] ids);
    }
}
