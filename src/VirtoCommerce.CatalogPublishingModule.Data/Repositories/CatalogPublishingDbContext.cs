using EntityFrameworkCore.Triggers;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.CatalogPublishingModule.Data.Model;

namespace VirtoCommerce.CatalogPublishingModule.Data.Repositories
{
    public class CatalogPublishingDbContext : DbContextWithTriggers
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
        }
    }
}
