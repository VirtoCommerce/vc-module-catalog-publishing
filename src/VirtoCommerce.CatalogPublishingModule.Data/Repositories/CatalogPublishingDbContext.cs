using System.Reflection;
using EntityFrameworkCore.Triggers;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.CatalogPublishingModule.Data.Model;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.CatalogPublishingModule.Data.Repositories
{
    public class CatalogPublishingDbContext : DbContextBase
    {
        public CatalogPublishingDbContext(DbContextOptions<CatalogPublishingDbContext> options)
            : base(options)
        {
        }

        protected CatalogPublishingDbContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CompletenessChannelEntity>().ToTable("CompletenessChannel").HasKey(x => x.Id);
            modelBuilder.Entity<CompletenessChannelEntity>()
                .Property(x => x.Id)
                .HasMaxLength(128)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<CompletenessChannelCurrencyEntity>().ToTable("CompletenessChannelCurrency").HasKey(x => x.Id);
            modelBuilder.Entity<CompletenessChannelCurrencyEntity>()
                .Property(x => x.Id)
                .HasMaxLength(128)
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<CompletenessChannelCurrencyEntity>()
                .HasOne(x => x.Channel)
                .WithMany(x => x.Currencies)
                .HasForeignKey(x => x.ChannelId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CompletenessChannelLanguageEntity>().ToTable("CompletenessChannelLanguage").HasKey(x => x.Id);
            modelBuilder.Entity<CompletenessChannelLanguageEntity>()
                .Property(x => x.Id)
                .HasMaxLength(128)
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<CompletenessChannelLanguageEntity>()
                .HasOne(x => x.Channel)
                .WithMany(x => x.Languages)
                .HasForeignKey(x => x.ChannelId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CompletenessEntryEntity>().ToTable("CompletenessEntry").HasKey(x => x.Id);
            modelBuilder.Entity<CompletenessEntryEntity>()
                .Property(x => x.Id)
                .HasMaxLength(128)
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<CompletenessEntryEntity>()
                .HasOne(x => x.Channel)
                .WithMany()
                .HasForeignKey(x => x.ChannelId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CompletenessDetailEntity>().ToTable("CompletenessDetail").HasKey(x => x.Id);
            modelBuilder.Entity<CompletenessDetailEntity>()
                .Property(x => x.Id)
                .HasMaxLength(128)
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<CompletenessDetailEntity>()
                .HasOne(x => x.CompletenessEntry)
                .WithMany(x => x.Details)
                .HasForeignKey(x => x.CompletenessEntryId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);


            // Allows configuration for an entity type for different database types.
            // Applies configuration from all <see cref="IEntityTypeConfiguration{TEntity}" in VirtoCommerce.CatalogPublishingModule.Data.XXX project. /> 
            switch (this.Database.ProviderName)
            {
                case "Pomelo.EntityFrameworkCore.MySql":
                    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.Load("VirtoCommerce.CatalogPublishingModule.Data.MySql"));
                    break;
                case "Npgsql.EntityFrameworkCore.PostgreSQL":
                    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.Load("VirtoCommerce.CatalogPublishingModule.Data.PostgreSql"));
                    break;
                case "Microsoft.EntityFrameworkCore.SqlServer":
                    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.Load("VirtoCommerce.CatalogPublishingModule.Data.SqlServer"));
                    break;
            }
        }
    }
}
