using System.Linq;
using VirtoCommerce.CatalogPublishingModule.Data.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogPublishingModule.Data.Repositories
{
    public interface IReadinessRepository : IRepository
    {
        IQueryable<ReadinessEntryEntity> Entries { get; }

        IQueryable<ReadinessDetailEntity> Details { get; }

        IQueryable<ReadinessChannelEntity> Channels { get; }

        ReadinessChannelEntity[] GetChannelsByIds(string[] ids);

        ReadinessEntryEntity[] GetEntriesByIds(string[] ids);

        void DeleteChannels(string[] ids);
    }
}