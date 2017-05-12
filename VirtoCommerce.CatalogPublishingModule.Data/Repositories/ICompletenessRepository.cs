using System.Linq;
using VirtoCommerce.CatalogPublishingModule.Data.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogPublishingModule.Data.Repositories
{
    public interface ICompletenessRepository : IRepository
    {
        IQueryable<CompletenessEntryEntity> Entries { get; }

        IQueryable<CompletenessDetailEntity> Details { get; }

        IQueryable<CompletenessChannelEntity> Channels { get; }

        CompletenessChannelEntity[] GetChannelsByIds(string[] ids);

        CompletenessEntryEntity[] GetEntriesByIds(string[] ids);

        void DeleteChannels(string[] ids);
    }
}