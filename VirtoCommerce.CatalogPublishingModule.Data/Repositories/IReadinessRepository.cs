using System.Linq;
using VirtoCommerce.CatalogPublishingModule.Data.Model;

namespace VirtoCommerce.CatalogPublishingModule.Data.Repositories
{
    public interface IReadinessRepository
    {
        IQueryable<ReadinessEntry> Entries { get; }

        IQueryable<ReadinessDetail> Details { get; }

        IQueryable<ReadinessChannel> Channels { get; }

        ReadinessChannel[] GetChannelsByIds(string[] ids);

        ReadinessEntry[] GetEntriesByIds(string[] ids);

        void DeleteChannels(string[] ids);
    }
}