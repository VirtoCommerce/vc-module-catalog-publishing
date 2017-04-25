using System.Data.Entity;
using System.Linq;
using VirtoCommerce.CatalogPublishingModule.Data.Model;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.Platform.Data.Infrastructure.Interceptors;

namespace VirtoCommerce.CatalogPublishingModule.Data.Repositories
{
    public class ReadinessRepositoryImpl : EFRepositoryBase, IReadinessRepository
    {
        public ReadinessRepositoryImpl(string nameOrConnectionString, params IInterceptor[] interceptors)
            : base(nameOrConnectionString, null, interceptors)
        {
            Configuration.LazyLoadingEnabled = false;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ReadinessDetailEntity>().HasKey(x => x.Id).Property(x => x.Id);
            modelBuilder.Entity<ReadinessDetailEntity>().HasRequired(x => x.ReadinessEntry).WithMany(x => x.Details).HasForeignKey(x => x.ReadinessEntryId).WillCascadeOnDelete(true);
            modelBuilder.Entity<ReadinessDetailEntity>().ToTable("ReadinessDetail");

            modelBuilder.Entity<ReadinessEntryEntity>().HasKey(x => x.Id).Property(x => x.Id);
            modelBuilder.Entity<ReadinessEntryEntity>().HasRequired(x => x.Channel).WithMany(x => x.Entries).HasForeignKey(x => x.ChannelId).WillCascadeOnDelete(true);
            modelBuilder.Entity<ReadinessEntryEntity>().ToTable("ReadinessEntry");

            modelBuilder.Entity<ReadinessChannelEntity>().HasKey(x => x.Id).Property(x => x.Id);
            modelBuilder.Entity<ReadinessChannelEntity>().ToTable("ReadinessChannel");

            base.OnModelCreating(modelBuilder);
        }

        public IQueryable<ReadinessEntryEntity> Entries
        {
            get { return GetAsQueryable<ReadinessEntryEntity>(); }
        }

        public IQueryable<ReadinessDetailEntity> Details
        {
            get { return GetAsQueryable<ReadinessDetailEntity>(); }
        }

        public IQueryable<ReadinessChannelEntity> Channels
        {
            get { return GetAsQueryable<ReadinessChannelEntity>(); }
        }

        public ReadinessChannelEntity[] GetChannelsByIds(string[] ids)
        {
            return Channels.Include(x => x.Entries).Where(x => ids.Contains(x.Id)).ToArray();
        }

        public ReadinessEntryEntity[] GetEntriesByIds(string[] ids)
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