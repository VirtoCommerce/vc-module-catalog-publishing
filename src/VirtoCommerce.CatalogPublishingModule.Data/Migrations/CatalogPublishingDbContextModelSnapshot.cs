// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using VirtoCommerce.CatalogPublishingModule.Data.Repositories;

namespace VirtoCommerce.CatalogPublishingModule.Data.Migrations
{
    [DbContext(typeof(CatalogPublishingDbContext))]
    partial class CatalogPublishingDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("VirtoCommerce.CatalogPublishingModule.Data.Model.CompletenessChannelCurrencyEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.Property<string>("ChannelId")
                        .IsRequired()
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("CompletenessChannelEntityId")
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("CurrencyCode")
                        .IsRequired()
                        .HasColumnType("nvarchar(64)")
                        .HasMaxLength(64);

                    b.HasKey("Id");

                    b.HasIndex("ChannelId");

                    b.HasIndex("CompletenessChannelEntityId");

                    b.ToTable("CompletenessChannelCurrency");
                });

            modelBuilder.Entity("VirtoCommerce.CatalogPublishingModule.Data.Model.CompletenessChannelEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.Property<string>("CatalogId")
                        .IsRequired()
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.Property<string>("CatalogName")
                        .IsRequired()
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.Property<string>("CreatedBy")
                        .HasColumnType("nvarchar(64)")
                        .HasMaxLength(64);

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("EvaluatorType")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ModifiedBy")
                        .HasColumnType("nvarchar(64)")
                        .HasMaxLength(64);

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.HasKey("Id");

                    b.ToTable("CompletenessChannel");
                });

            modelBuilder.Entity("VirtoCommerce.CatalogPublishingModule.Data.Model.CompletenessChannelLanguageEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.Property<string>("ChannelId")
                        .IsRequired()
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("CompletenessChannelEntityId")
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("LanguageCode")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("ChannelId");

                    b.HasIndex("CompletenessChannelEntityId");

                    b.ToTable("CompletenessChannelLanguage");
                });

            modelBuilder.Entity("VirtoCommerce.CatalogPublishingModule.Data.Model.CompletenessDetailEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.Property<string>("CompletenessEntryEntityId")
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("CompletenessEntryId")
                        .IsRequired()
                        .HasColumnType("nvarchar(128)");

                    b.Property<int>("CompletenessPercent")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.HasKey("Id");

                    b.HasIndex("CompletenessEntryEntityId");

                    b.HasIndex("CompletenessEntryId");

                    b.ToTable("CompletenessDetail");
                });

            modelBuilder.Entity("VirtoCommerce.CatalogPublishingModule.Data.Model.CompletenessEntryEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.Property<string>("ChannelId")
                        .IsRequired()
                        .HasColumnType("nvarchar(128)");

                    b.Property<int>("CompletenessPercent")
                        .HasColumnType("int");

                    b.Property<string>("CreatedBy")
                        .HasColumnType("nvarchar(64)")
                        .HasMaxLength(64);

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("ModifiedBy")
                        .HasColumnType("nvarchar(64)")
                        .HasMaxLength(64);

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("ProductId")
                        .IsRequired()
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.HasKey("Id");

                    b.HasIndex("ChannelId");

                    b.ToTable("CompletenessEntry");
                });

            modelBuilder.Entity("VirtoCommerce.CatalogPublishingModule.Data.Model.CompletenessChannelCurrencyEntity", b =>
                {
                    b.HasOne("VirtoCommerce.CatalogPublishingModule.Data.Model.CompletenessChannelEntity", "Channel")
                        .WithMany()
                        .HasForeignKey("ChannelId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("VirtoCommerce.CatalogPublishingModule.Data.Model.CompletenessChannelEntity", null)
                        .WithMany("Currencies")
                        .HasForeignKey("CompletenessChannelEntityId");
                });

            modelBuilder.Entity("VirtoCommerce.CatalogPublishingModule.Data.Model.CompletenessChannelLanguageEntity", b =>
                {
                    b.HasOne("VirtoCommerce.CatalogPublishingModule.Data.Model.CompletenessChannelEntity", "Channel")
                        .WithMany()
                        .HasForeignKey("ChannelId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("VirtoCommerce.CatalogPublishingModule.Data.Model.CompletenessChannelEntity", null)
                        .WithMany("Languages")
                        .HasForeignKey("CompletenessChannelEntityId");
                });

            modelBuilder.Entity("VirtoCommerce.CatalogPublishingModule.Data.Model.CompletenessDetailEntity", b =>
                {
                    b.HasOne("VirtoCommerce.CatalogPublishingModule.Data.Model.CompletenessEntryEntity", null)
                        .WithMany("Details")
                        .HasForeignKey("CompletenessEntryEntityId");

                    b.HasOne("VirtoCommerce.CatalogPublishingModule.Data.Model.CompletenessEntryEntity", "CompletenessEntry")
                        .WithMany()
                        .HasForeignKey("CompletenessEntryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("VirtoCommerce.CatalogPublishingModule.Data.Model.CompletenessEntryEntity", b =>
                {
                    b.HasOne("VirtoCommerce.CatalogPublishingModule.Data.Model.CompletenessChannelEntity", "Channel")
                        .WithMany()
                        .HasForeignKey("ChannelId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}