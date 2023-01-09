using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.CatalogPublishingModule.Data.Model;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.CatalogPublishingModule.Data.Repositories
{
    public class CompletenessRepositoryImpl : DbContextRepositoryBase<CatalogPublishingDbContext>, ICompletenessRepository
    {
        public CompletenessRepositoryImpl(CatalogPublishingDbContext dbContext) : base(dbContext)
        {
        }

        public IQueryable<CompletenessEntryEntity> Entries => DbContext.Set<CompletenessEntryEntity>();
        public IQueryable<CompletenessDetailEntity> Details => DbContext.Set<CompletenessDetailEntity>();
        public IQueryable<CompletenessChannelEntity> Channels => DbContext.Set<CompletenessChannelEntity>().Include(x => x.Languages).Include(x => x.Currencies);

        public Task<CompletenessChannelEntity[]> GetChannelsByIdsAsync(string[] ids)
        {
            return Channels.Where(x => ids.Contains(x.Id)).ToArrayAsync();
        }

        public Task<CompletenessEntryEntity[]> GetEntriesByIdsAsync(string[] ids)
        {
            return Entries.Include(x => x.Channel).Include(x => x.Details).Where(x => ids.Contains(x.Id)).ToArrayAsync();
        }

        public async Task DeleteChannelsAsync(string[] ids)
        {
            var channels = await GetChannelsByIdsAsync(ids);

            foreach (var channel in channels)
            {
                Remove(channel);
            }
        }
    }
}
