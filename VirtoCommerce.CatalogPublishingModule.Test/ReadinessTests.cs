using System;
using System.Linq;
using VirtoCommerce.CatalogPublishingModule.Core.Model;
using VirtoCommerce.CatalogPublishingModule.Core.Model.Search;
using VirtoCommerce.CatalogPublishingModule.Core.Services;
using VirtoCommerce.CatalogPublishingModule.Data.Repositories;
using VirtoCommerce.CatalogPublishingModule.Data.Services;
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
            var service = GetService();
            var channel = GetChannel();
            ReadinessChannel testChannel;
            
            service.SaveChannels(new [] { channel });
            testChannel = service.GetChannelsByIds(new[] { channel.Id }).FirstOrDefault();
            Assert.True(CompareChannels(channel, testChannel));
            
            channel.Name = "Changed";
            service.SaveChannels(new [] { channel });
            testChannel = service.GetChannelsByIds(new[] { channel.Id }).FirstOrDefault();
            Assert.True(CompareChannels(channel, testChannel));
            
            testChannel = service.SearchChannels(new ReadinessChannelSearchCriteria()).Results.FirstOrDefault();
            Assert.True(CompareChannels(channel, testChannel));

            service.DeleteChannels(new [] { channel.Id });
            testChannel = service.GetChannelsByIds(new[] { channel.Id }).FirstOrDefault();
            Assert.True(CompareChannels(testChannel, null));
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
                EvaluatorType = "DefaultEvaluatorType",
                ReadinessPercent = 50,
                CreatedDate = DateTime.Now,
                CreatedBy = "Test"
            };
        }

        private bool CompareChannels(ReadinessChannel first, ReadinessChannel second)
        {
            if (first == null && second == null)
            {
                return true;
            }
            if (first == null || second == null)
            {
                return false;
            }
            // Do not include ReadinessPercent here, because it calculated in runtime and not saved to database
            return first.Id == second.Id &&
                   first.Name == second.Name &&
                   first.Language == second.Language &&
                   first.PricelistId == second.PricelistId &&
                   first.CatalogId == second.CatalogId &&
                   first.EvaluatorType == second.EvaluatorType;
        }

        private IReadinessService GetService()
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