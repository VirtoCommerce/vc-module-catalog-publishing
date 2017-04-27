using System;
using System.Data.Entity;
using System.Linq;
using VirtoCommerce.CatalogPublishingModule.Core.Model;
using VirtoCommerce.CatalogPublishingModule.Core.Model.Search;
using VirtoCommerce.CatalogPublishingModule.Core.Services;
using VirtoCommerce.CatalogPublishingModule.Data.Model;
using VirtoCommerce.CatalogPublishingModule.Data.Repositories;
using VirtoCommerce.CatalogPublishingModule.Data.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure.Interceptors;
using Xunit;

namespace VirtoCommerce.CatalogPublishingModule.Test
{
    [Rollback]
    public class ReadinessTests
    {
        [Fact]
        public void ChannelTest()
        {
            var service = GetReadinessService();
            var channel = GetChannel();
            ReadinessChannel testChannel;

            service.SaveChannels(new[] { channel });
            testChannel = service.GetChannelsByIds(new[] { channel.Id }).FirstOrDefault();
            Assert.True(CompareChannels(channel, testChannel));

            channel.Name = "Changed";
            service.SaveChannels(new[] { channel });
            testChannel = service.GetChannelsByIds(new[] { channel.Id }).FirstOrDefault();
            Assert.True(CompareChannels(channel, testChannel));

            testChannel = service.SearchChannels(new ReadinessChannelSearchCriteria { CatalogIds = new [] { "Test" } }).Results.FirstOrDefault();
            Assert.True(CompareChannels(channel, testChannel));

            service.DeleteChannels(new[] { channel.Id });
            testChannel = service.GetChannelsByIds(new[] { channel.Id }).FirstOrDefault();
            Assert.True(CompareChannels(testChannel, null));
        }

        [Fact]
        public void EntryTest()
        {
            var service = GetReadinessService();
            var channel = GetChannel();
            var entry = GetEntry();
            ReadinessChannel testChannel;

            using (var repository = GetRepository())
            {
                service.SaveChannels(new[] { channel });

                TestEntry(repository, service, entry);
            }

            testChannel = service.GetChannelsByIds(new[] { channel.Id }).FirstOrDefault();
            Assert.True(testChannel != null && testChannel.ReadinessPercent == entry.ReadinessPercent);

            // Changed
            using (var repository = GetRepository())
            {
                Array.ForEach(entry.Details, x => x.ReadinessPercent = 15);
                entry.ReadinessPercent = (int)Math.Round((double)entry.Details.Sum(x => x.ReadinessPercent) / entry.Details.Length);

                service.SaveEntries(new[] { entry });

                TestEntry(repository, service, entry);
            }

            testChannel = service.GetChannelsByIds(new[] { channel.Id }).FirstOrDefault();
            Assert.True(testChannel != null && testChannel.ReadinessPercent == entry.ReadinessPercent);
        }

        private void TestEntry(IReadinessRepository repository, IReadinessService service, ReadinessEntry correctEntry)
        {
            service.SaveEntries(new[] { correctEntry });
            var testEntryEntity = repository.Entries.Include(x => x.Details).FirstOrDefault(x => x.ChannelId == correctEntry.ChannelId);
            var testEntry = testEntryEntity != null ? testEntryEntity.ToModel(AbstractTypeFactory<ReadinessEntry>.TryCreateInstance()) : null;
            Assert.True(CompareEntries(testEntry, correctEntry));

            testEntryEntity = testEntryEntity != null ? repository.GetEntriesByIds(new[] { testEntryEntity.Id }).FirstOrDefault() : null;
            testEntry = testEntryEntity != null ? testEntryEntity.ToModel(AbstractTypeFactory<ReadinessEntry>.TryCreateInstance()) : null;
            Assert.True(CompareEntries(testEntry, correctEntry));
        }

        [Fact]
        private void DetailsTest()
        {
            var service = GetReadinessService();
            var channel = GetChannel();
            var entry = GetEntry();

            using (var repository = GetRepository())
            {
                service.SaveChannels(new[] { channel });
                service.SaveEntries(new[] { entry });

                var testEntryEntity = repository.Entries.Include(x => x.Details).FirstOrDefault(x => x.ChannelId == entry.ChannelId);
                var testDetailsEntities = repository.Details.Include(x => x.ReadinessEntry).ToArray().Where(x => x.ReadinessEntryId == (testEntryEntity != null ? testEntryEntity.Id : null)).ToArray();
                var testDetails = testDetailsEntities.Select(x => x.ToModel(AbstractTypeFactory<ReadinessDetail>.TryCreateInstance())).ToArray();
                Assert.True(CompareDetails(entry.Details, testDetails));
            }
        }

        [Fact]
        public void EntitiesTests()
        {
            var channel = new ReadinessChannelEntity();
            var entry = new ReadinessEntryEntity();
            var detail = new ReadinessDetailEntity();

            Assert.Throws<ArgumentNullException>(() => channel.ToModel(null));
            Assert.Throws<ArgumentNullException>(() => channel.FromModel(null, new PrimaryKeyResolvingMap()));
            Assert.Throws<ArgumentNullException>(() => channel.FromModel(new ReadinessChannel(), null));
            Assert.Throws<ArgumentNullException>(() => channel.Patch(null));

            Assert.Throws<ArgumentNullException>(() => entry.ToModel(null));
            Assert.Throws<ArgumentNullException>(() => entry.FromModel(null)); ;
            Assert.Throws<ArgumentNullException>(() => entry.Patch(null));

            Assert.Throws<ArgumentNullException>(() => detail.ToModel(null));
            Assert.Throws<ArgumentNullException>(() => detail.FromModel(null));
            Assert.Throws<ArgumentNullException>(() => detail.Patch(null));
        }

        private ReadinessChannel GetChannel()
        {
            return new ReadinessChannel
            {
                Id = "Test",
                Name = "Test",
                Language = "en",
                PricelistId = "Test",
                CatalogId = "Test",
                CatalogName = "Test",
                EvaluatorType = "DefaultEvaluatorType",
                ReadinessPercent = 50,
                CreatedDate = DateTime.Now,
                CreatedBy = "Test"
            };
        }

        private bool CompareChannels(ReadinessChannel first, ReadinessChannel second)
        {
            // Do not include ReadinessPercent here, because it calculated in runtime and not saved to database
            return Equals(first, second) ||
                   first.Id == second.Id &&
                   first.Name == second.Name &&
                   first.Language == second.Language &&
                   first.PricelistId == second.PricelistId &&
                   first.CatalogId == second.CatalogId &&
                   first.EvaluatorType == second.EvaluatorType;
        }

        private ReadinessEntry GetEntry()
        {
            return new ReadinessEntry
            {
                ChannelId = "Test",
                ProductId = "Test",
                ReadinessPercent = 50,
                Details = new[]
                {
                    new ReadinessDetail
                    {
                        Name = "Test",
                        ReadinessPercent = 25
                    },
                    new ReadinessDetail
                    {
                        Name = "Test2",
                        ReadinessPercent = 25
                    }
                }
            };
        }

        private bool CompareEntries(ReadinessEntry first, ReadinessEntry second)
        {
            return Equals(first, second) ||
                   first.ChannelId == second.ChannelId &&
                   first.ProductId == second.ProductId &&
                   first.ReadinessPercent == second.ReadinessPercent &&
                   CompareDetails(first.Details, second.Details);
        }

        private bool CompareDetails(ReadinessDetail[] first, ReadinessDetail[] second)
        {
            return first.Length == second.Length && first.All(x => second.Any(y => CompareDetail(x, y)));
        }

        private bool CompareDetail(ReadinessDetail first, ReadinessDetail second)
        {
            return Equals(first, second) ||
                   first.Name == second.Name &&
                   first.ReadinessPercent == second.ReadinessPercent;
        }

        private IReadinessService GetReadinessService()
        {
            return new ReadinessServiceImpl(GetRepository);
        }

        private static IReadinessRepository GetRepository()
        {
            var repository = new ReadinessRepositoryImpl("VirtoCommerce", new EntityPrimaryKeyGeneratorInterceptor());
            return repository;
        }
    }
}