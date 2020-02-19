//using System;
//using System.Collections.Generic;
//using System.Data.Entity;
//using System.Linq;
//using System.Threading.Tasks;
//using VirtoCommerce.CatalogPublishingModule.Core.Model;
//using VirtoCommerce.CatalogPublishingModule.Core.Model.Search;
//using VirtoCommerce.CatalogPublishingModule.Core.Services;
//using VirtoCommerce.CatalogPublishingModule.Data.Model;
//using VirtoCommerce.CatalogPublishingModule.Data.Repositories;
//using VirtoCommerce.CatalogPublishingModule.Data.Services;
//using VirtoCommerce.Platform.Core.Common;
//using VirtoCommerce.Platform.Data.Infrastructure.Interceptors;
//using Xunit;

//namespace VirtoCommerce.CatalogPublishingModule.Test
//{
//    [Rollback]
//    public class CompletenessTests
//    {
//        [Fact]
//        public async Task ChannelTest()
//        {
//            var service = GetCompletenessService();
//            var channel = GetChannel();
//            CompletenessChannel testChannel;

//            await service.SaveChannelsAsync(new[] { channel });
//            testChannel = (await service.GetChannelsByIdsAsync(new[] { channel.Id })).FirstOrDefault();
//            Assert.True(CompareChannels(channel, testChannel));

//            channel.Name = "Changed";
//            await service.SaveChannelsAsync(new[] { channel });
//            testChannel = (await service.GetChannelsByIdsAsync(new[] { channel.Id })).FirstOrDefault();
//            Assert.True(CompareChannels(channel, testChannel));

//            testChannel = (await service.SearchChannelsAsync(new CompletenessChannelSearchCriteria { CatalogIds = new [] { "Test" } })).Results.FirstOrDefault();
//            Assert.True(CompareChannels(channel, testChannel));

//            await service.DeleteChannelsAsync(new[] { channel.Id });
//            testChannel = (await service.GetChannelsByIdsAsync(new[] { channel.Id })).FirstOrDefault();
//            Assert.True(CompareChannels(testChannel, null));
//        }

//        [Fact]
//        public async Task EntryTest()
//        {
//            var service = GetCompletenessService();
//            var channel = GetChannel();
//            var entry = GetEntry();
//            CompletenessChannel testChannel;

//            // Added
//            service.SaveChannels(new[] { channel });
//            TestEntry(service, entry);

//            testChannel = service.GetChannelsByIds(new[] { channel.Id }).FirstOrDefault();
//            Assert.True(testChannel != null && testChannel.CompletenessPercent == entry.CompletenessPercent);

//            // Changed
//            Array.ForEach(entry.Details, x => x.CompletenessPercent = 15);
//            entry.CompletenessPercent = (int) Math.Floor((double) entry.Details.Sum(x => x.CompletenessPercent) / entry.Details.Length);
//            TestEntry(service, entry);

//            testChannel = service.GetChannelsByIds(new[] { channel.Id }).FirstOrDefault();
//            Assert.True(testChannel != null && testChannel.CompletenessPercent == entry.CompletenessPercent);
//        }

//        private void TestEntry(ICompletenessService service, CompletenessEntry correctEntry)
//        {
//            service.SaveEntries(new[] { correctEntry });
//            var testEntry = service.GetCompletenessEntriesByIds(new[] { correctEntry.Id }).FirstOrDefault();
//            Assert.True(CompareEntries(testEntry, correctEntry));
//        }

//        [Fact]
//        private async Task DetailsTest()
//        {
//            var service = GetCompletenessService();
//            var channel = GetChannel();
//            var entry = GetEntry();

//            using (var repository = GetRepository())
//            {
//                service.SaveChannels(new[] { channel });
//                service.SaveEntries(new[] { entry });

//                var testEntryEntity = repository.Entries.Include(x => x.Details).FirstOrDefault(x => x.ChannelId == entry.ChannelId);
//                var testDetailsEntities = repository.Details.ToArray().Where(x => x.CompletenessEntryId == (testEntryEntity != null ? testEntryEntity.Id : null)).ToArray();
//                var testDetails = testDetailsEntities.Select(x => x.ToModel(AbstractTypeFactory<CompletenessDetail>.TryCreateInstance())).ToArray();
//                Assert.True(CompareDetails(entry.Details, testDetails));
//            }
//        }

//        [Fact]
//        public async Task EntitiesTests()
//        {
//            var channel = new CompletenessChannelEntity();
//            var entry = new CompletenessEntryEntity();
//            var detail = new CompletenessDetailEntity();

//            Assert.Throws<ArgumentNullException>(() => channel.ToModel(null));
//            Assert.Throws<ArgumentNullException>(() => channel.FromModel(null, new PrimaryKeyResolvingMap()));
//            Assert.Throws<ArgumentNullException>(() => channel.FromModel(new CompletenessChannel(), null));
//            Assert.Throws<ArgumentNullException>(() => channel.Patch(null));

//            Assert.Throws<ArgumentNullException>(() => entry.ToModel(null));
//            Assert.Throws<ArgumentNullException>(() => entry.FromModel(null));
//            Assert.Throws<ArgumentNullException>(() => entry.Patch(null));

//            Assert.Throws<ArgumentNullException>(() => detail.ToModel(null));
//            Assert.Throws<ArgumentNullException>(() => detail.FromModel(null));
//            Assert.Throws<ArgumentNullException>(() => detail.Patch(null));
//        }

//        private CompletenessChannel GetChannel()
//        {
//            return new CompletenessChannel
//            {
//                Id = "Test",
//                Name = "Test",
//                Languages = new List<string> { "Test1", "Test2" },
//                Currencies = new List<string> { "Test1", "Test2" },
//                CatalogId = "Test",
//                CatalogName = "Test",
//                EvaluatorType = "DefaultEvaluatorType",
//                CompletenessPercent = 50,
//                CreatedDate = DateTime.Now,
//                CreatedBy = "Test"
//            };
//        }

//        private bool CompareChannels(CompletenessChannel first, CompletenessChannel second)
//        {
//            // Do not include CompletenessPercent here, because it calculated in runtime and not saved to database
//            return first == null && second == null || first != null && second != null &&
//                   first.Id == second.Id &&
//                   first.Name == second.Name &&
//                   first.Languages.Count == second.Languages.Count && !first.Languages.Except(second.Languages).Any() &&
//                   first.Currencies.Count == second.Currencies.Count && !first.Currencies.Except(second.Currencies).Any() &&
//                   first.CatalogId == second.CatalogId &&
//                   first.EvaluatorType == second.EvaluatorType;
//        }

//        private CompletenessEntry GetEntry()
//        {
//            return new CompletenessEntry
//            {
//                Id = "Test",
//                CreatedBy = "Test",
//                CreatedDate = DateTime.Now,
//                ModifiedBy = null,
//                ModifiedDate = null,
//                ChannelId = "Test",
//                ProductId = "Test",
//                CompletenessPercent = 50,
//                Details = new[]
//                {
//                    new CompletenessDetail
//                    {
//                        Name= "Test1",
//                        CompletenessPercent = 25
//                    },
//                    new CompletenessDetail
//                    {
//                        Name= "Test2",
//                        CompletenessPercent = 25
//                    },
//                    new CompletenessDetail
//                    {
//                        Name= "Test3",
//                        CompletenessPercent = 25
//                    },
//                    new CompletenessDetail
//                    {
//                        Name= "Test4",
//                        CompletenessPercent = 25
//                    }
//                }
//            };
//        }

//        private bool CompareEntries(CompletenessEntry first, CompletenessEntry second)
//        {
//            return first == null && second == null || first != null && second != null &&
//                   first.ChannelId == second.ChannelId &&
//                   first.ProductId == second.ProductId &&
//                   first.CompletenessPercent == second.CompletenessPercent &&
//                   CompareDetails(first.Details, second.Details);
//        }

//        private bool CompareDetails(CompletenessDetail[] first, CompletenessDetail[] second)
//        {
//            return first.Length == second.Length && first.All(x => second.Any(y => CompareDetail(x, y)));
//        }

//        private bool CompareDetail(CompletenessDetail first, CompletenessDetail second)
//        {
//            return first == null && second == null || first != null && second != null && first.Name == second.Name && first.CompletenessPercent == second.CompletenessPercent;
//        }

//        private ICompletenessService GetCompletenessService()
//        {
//            return new CompletenessServiceImpl(GetRepository);
//        }

//        private static ICompletenessRepository GetRepository()
//        {
//            var repository = new CompletenessRepositoryImpl("VirtoCommerce", new EntityPrimaryKeyGeneratorInterceptor());
//            return repository;
//        }
//    }
//}
