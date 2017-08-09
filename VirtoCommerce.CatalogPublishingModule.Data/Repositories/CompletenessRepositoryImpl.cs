using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using VirtoCommerce.CatalogPublishingModule.Data.Model;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.Platform.Data.Infrastructure.Interceptors;

namespace VirtoCommerce.CatalogPublishingModule.Data.Repositories
{
    public class CompletenessRepositoryImpl : EFRepositoryBase, ICompletenessRepository
    {
        public CompletenessRepositoryImpl()
        {
        }

        public CompletenessRepositoryImpl(string nameOrConnectionString, params IInterceptor[] interceptors)
            : base(nameOrConnectionString, null, interceptors)
        {
            Configuration.LazyLoadingEnabled = false;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CompletenessDetailEntity>().HasKey(x => x.Id).Property(x => x.Id);
            modelBuilder.Entity<CompletenessDetailEntity>().HasRequired(x => x.CompletenessEntry).WithMany(x => x.Details).HasForeignKey(x => x.CompletenessEntryId).WillCascadeOnDelete(true);
            modelBuilder.Entity<CompletenessDetailEntity>().ToTable("CompletenessDetail");

            modelBuilder.Entity<CompletenessEntryEntity>().HasKey(x => x.Id).Property(x => x.Id);
            modelBuilder.Entity<CompletenessEntryEntity>().HasRequired(x => x.Channel).WithMany().HasForeignKey(x => x.ChannelId).WillCascadeOnDelete(true);
            modelBuilder.Entity<CompletenessEntryEntity>().ToTable("CompletenessEntry");

            modelBuilder.Entity<CompletenessChannelLanguageEntity>().HasKey(x => x.Id).Property(x => x.Id);
            modelBuilder.Entity<CompletenessChannelLanguageEntity>().HasRequired(x => x.Channel).WithMany(x => x.Languages).HasForeignKey(x => x.ChannelId).WillCascadeOnDelete(true);
            modelBuilder.Entity<CompletenessChannelLanguageEntity>().ToTable("CompletenessChannelLanguage");

            modelBuilder.Entity<CompletenessChannelCurrencyEntity>().HasKey(x => x.Id).Property(x => x.Id);
            modelBuilder.Entity<CompletenessChannelCurrencyEntity>().HasRequired(x => x.Channel).WithMany(x => x.Currencies).HasForeignKey(x => x.ChannelId).WillCascadeOnDelete(true);
            modelBuilder.Entity<CompletenessChannelCurrencyEntity>().ToTable("CompletenessChannelCurrency");

            modelBuilder.Entity<CompletenessChannelEntity>().HasKey(x => x.Id).Property(x => x.Id);
            modelBuilder.Entity<CompletenessChannelEntity>().ToTable("CompletenessChannel");

            base.OnModelCreating(modelBuilder);
        }

        public IQueryable<CompletenessEntryEntity> Entries => GetAsQueryable<CompletenessEntryEntity>();
        public IQueryable<CompletenessDetailEntity> Details => GetAsQueryable<CompletenessDetailEntity>();
        public IQueryable<CompletenessChannelEntity> Channels => GetAsQueryable<CompletenessChannelEntity>().Include(x => x.Languages).Include(x => x.Currencies);

        public CompletenessChannelEntity[] GetChannelsByIds(string[] ids)
        {
            return Channels.Where(x => ids.Contains(x.Id)).ToArray();
        }

        public CompletenessEntryEntity[] GetEntriesByIds(string[] ids)
        {
            return Entries.Include(x => x.Channel).Include(x => x.Details).Where(x => ids.Contains(x.Id)).ToArray();
        }

        public void DeleteChannels(string[] ids)
        {
            ExecuteStoreCommand("DELETE FROM CompletenessChannel WHERE Id IN ({0})", ids);
        }


        protected virtual void ExecuteStoreCommand(string commandTemplate, IEnumerable<string> parameterValues)
        {
            var command = CreateCommand(commandTemplate, parameterValues);
            ObjectContext.ExecuteStoreCommand(command.Text, command.Parameters);
        }

        protected virtual Command CreateCommand(string commandTemplate, IEnumerable<string> parameterValues)
        {
            var parameters = parameterValues.Select((v, i) => new SqlParameter($"@p{i}", v)).ToArray();
            var parameterNames = string.Join(",", parameters.Select(p => p.ParameterName));

            return new Command
            {
                Text = string.Format(commandTemplate, parameterNames),
                Parameters = parameters.OfType<object>().ToArray(),
            };
        }

        protected class Command
        {
            public string Text { get; set; }
            public object[] Parameters { get; set; }
        }
    }
}
