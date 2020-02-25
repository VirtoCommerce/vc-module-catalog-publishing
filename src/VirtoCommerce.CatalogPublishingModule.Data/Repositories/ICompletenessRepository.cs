using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogPublishingModule.Data.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogPublishingModule.Data.Repositories
{
    public interface ICompletenessRepository : IRepository
    {
        IQueryable<CompletenessEntryEntity> Entries { get; }

        IQueryable<CompletenessDetailEntity> Details { get; }

        IQueryable<CompletenessChannelEntity> Channels { get; }

        Task<CompletenessChannelEntity[]> GetChannelsByIdsAsync(string[] ids);

        Task<CompletenessEntryEntity[]> GetEntriesByIdsAsync(string[] ids);

        Task DeleteChannelsAsync(string[] ids);
    }
}
