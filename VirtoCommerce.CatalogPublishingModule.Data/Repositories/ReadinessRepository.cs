using System.Data.Entity;
using System.Linq;
using VirtoCommerce.CatalogPublishingModule.Data.Model;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.Platform.Data.Infrastructure.Interceptors;

namespace VirtoCommerce.CatalogPublishingModule.Data.Repositories
{
    public class ReadinessRepository : EFRepositoryBase, IReadinessRepository
    {
        public ReadinessRepository()
        {
        }

        public ReadinessRepository(string nameOrConnectionString, params IInterceptor[] interceptors)
            : base(nameOrConnectionString, null, interceptors)
        {
            Configuration.LazyLoadingEnabled = false;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ReadinessDetail>().HasKey(x => x.Id).Property(x => x.Id);
            modelBuilder.Entity<ReadinessDetail>().HasRequired(x => x.ReadinessEntry).WithMany(x => x.Details).HasForeignKey(x => x.ReadinessEntryId).WillCascadeOnDelete(true);
            modelBuilder.Entity<ReadinessDetail>().ToTable("ReadinessDetail");

            modelBuilder.Entity<ReadinessEntry>().HasKey(x => x.Id).Property(x => x.Id);
            modelBuilder.Entity<ReadinessEntry>().HasRequired(x => x.Channel).WithMany().HasForeignKey(x => x.ChannelId).WillCascadeOnDelete(true);
            modelBuilder.Entity<ReadinessEntry>().ToTable("ReadinessEntry");

            modelBuilder.Entity<ReadinessChannel>().HasKey(x => x.Id).Property(x => x.Id);
            modelBuilder.Entity<ReadinessChannel>().ToTable("ReadinessChannel");

            base.OnModelCreating(modelBuilder);
        }

        public IQueryable<ReadinessEntry> Entries
        {
            get { return GetAsQueryable<ReadinessEntry>(); }
        }

        public IQueryable<ReadinessDetail> Details
        {
            get { return GetAsQueryable<ReadinessDetail>(); }
        }

        public IQueryable<ReadinessChannel> Channels
        {
            get { return GetAsQueryable<ReadinessChannel>(); }
        }

        public ReadinessChannel[] GetChannelsByIds(string[] ids)
        {
            return Channels.Where(x => ids.Contains(x.Id)).ToArray();
        }

        public ReadinessEntry[] GetEntriesByIds(string[] ids)
        {
            return Entries.Include(x => x.Details).Where(x => ids.Contains(x.Id)).ToArray();
        }

        public void DeleteChannels(string[] ids)
        {
            var queryPattern = @"DELETE FROM ReadinessChannel WHERE Id IN ({0})";
            var query = string.Format(queryPattern, string.Join(", ", ids.Select(x => string.Format("'{0}'", x))));
            ObjectContext.ExecuteStoreCommand(query);
        }
    }
}