using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using VirtoCommerce.CatalogPublishingModule.Core.Model;
using VirtoCommerce.CatalogPublishingModule.Core.Model.Search;
using VirtoCommerce.CatalogPublishingModule.Core.Services;
using VirtoCommerce.CatalogPublishingModule.Data.Caching;
using VirtoCommerce.CatalogPublishingModule.Data.Model;
using VirtoCommerce.CatalogPublishingModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.CatalogPublishingModule.Data.Services
{
    public class CompletenessServiceImpl : ICompletenessService
    {
        private readonly Func<ICompletenessRepository> _repositoryFactory;
        private readonly IPlatformMemoryCache _platformMemoryCache;

        public CompletenessServiceImpl(Func<ICompletenessRepository> repositoryFactory, IPlatformMemoryCache platformMemoryCache)
        {
            _repositoryFactory = repositoryFactory;
            _platformMemoryCache = platformMemoryCache;
        }

        public async Task<CompletenessChannel[]> GetChannelsByIdsAsync(string[] ids)
        {
            var cacheKey = CacheKey.With(GetType(), nameof(GetChannelsByIdsAsync), string.Join("-", ids));

            return await _platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey, async (cacheEntry) =>
            {
                cacheEntry.AddExpirationToken(CompletenessCacheRegion.CreateChangeToken());

                CompletenessChannel[] result = null;
                if (ids != null)
                {
                    using (var repository = _repositoryFactory())
                    {
                        repository.DisableChangesTracking();

                        var channelEntities = await repository.GetChannelsByIdsAsync(ids);
                        result = channelEntities.Select(x =>
                        {
                            var channel = x.ToModel(AbstractTypeFactory<CompletenessChannel>.TryCreateInstance());
                            channel.CompletenessPercent = (int)Math.Floor(repository.Entries.Where(e => e.ChannelId == x.Id).Average(e => (int?)e.CompletenessPercent) ?? 0);
                            return channel;
                        }).ToArray();
                    }
                }
                return result;
            });
        }

        public async Task<CompletenessEntry[]> GetCompletenessEntriesByIdsAsync(string[] ids)
        {
            var cacheKey = CacheKey.With(GetType(), nameof(GetCompletenessEntriesByIdsAsync), string.Join("-", ids));

            return await _platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey, async (cacheEntry) =>
            {
                cacheEntry.AddExpirationToken(CompletenessCacheRegion.CreateChangeToken());

                CompletenessEntry[] result = null;
                if (ids != null)
                {
                    using (var repository = _repositoryFactory())
                    {
                        repository.DisableChangesTracking();

                        var entryEntities = await repository.GetEntriesByIdsAsync(ids);
                        result = entryEntities.Select(x => x.ToModel(AbstractTypeFactory<CompletenessEntry>.TryCreateInstance())).ToArray();
                    }
                }
                return result;
            });
        }

        public async Task SaveChannelsAsync(CompletenessChannel[] channels)
        {
            var pkMap = new PrimaryKeyResolvingMap();

            using (var repository = _repositoryFactory())
            {
                var ids = channels.Select(x => x.Id).Where(x => x != null).Distinct().ToArray();
                var alreadyExistEntities = await repository.Channels.Where(x => ids.Contains(x.Id)).ToArrayAsync();

                foreach (var channel in channels)
                {
                    var sourceEntity = AbstractTypeFactory<CompletenessChannelEntity>.TryCreateInstance().FromModel(channel, pkMap);
                    var targetEntity = alreadyExistEntities.FirstOrDefault(x => x.Id == channel.Id);
                    if (targetEntity != null)
                    {
                        sourceEntity.Patch(targetEntity);
                    }
                    else
                    {
                        repository.Add(sourceEntity);
                    }
                }

                await repository.UnitOfWork.CommitAsync();

                pkMap.ResolvePrimaryKeys();

                CompletenessCacheRegion.ExpireRegion();
            }
        }

        public async Task SaveEntriesAsync(CompletenessEntry[] entries)
        {
            using (var repository = _repositoryFactory())
            {
                var allDbEntries = await repository.Entries.Include(x => x.Details).ToArrayAsync();
                var alreadyExistEntities = allDbEntries.Where(x => entries.Any(y => CompareEntries(y, x))).ToArray();

                foreach (var entry in entries)
                {
                    var sourceEntity = AbstractTypeFactory<CompletenessEntryEntity>.TryCreateInstance().FromModel(entry);
                    var targetEntity = alreadyExistEntities.FirstOrDefault(x => CompareEntries(entry, x));
                    if (targetEntity != null)
                    {
                        sourceEntity.Patch(targetEntity);
                    }
                    else
                    {
                        repository.Add(sourceEntity);
                    }
                }

                await repository.UnitOfWork.CommitAsync();

                CompletenessCacheRegion.ExpireRegion();
            }
        }

        public async Task<CompletenessChannelSearchResult> SearchChannelsAsync(CompletenessChannelSearchCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException(nameof(criteria));
            }

            var cacheKey = CacheKey.With(GetType(), nameof(SearchChannelsAsync), criteria.GetCacheKey());

            return await _platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey, async (cacheEntry) =>
            {
                cacheEntry.AddExpirationToken(CompletenessCacheRegion.CreateChangeToken());

                var result = AbstractTypeFactory<CompletenessChannelSearchResult>.TryCreateInstance();
                using (var repository = _repositoryFactory())
                {
                    repository.DisableChangesTracking();

                    var query = repository.Channels;
                    if (!criteria.CatalogIds.IsNullOrEmpty())
                    {
                        query = query.Where(x => criteria.CatalogIds.Contains(x.CatalogId));
                    }

                    var sortInfos = criteria.SortInfos;
                    if (sortInfos.IsNullOrEmpty())
                    {
                        sortInfos = new[] { new SortInfo { SortColumn = ReflectionUtility.GetPropertyName<CompletenessChannel>(x => x.Name) } };
                    }
                    query = query.OrderBySortInfos(sortInfos);

                    result.TotalCount = query.Count();
                    query = query.Skip(criteria.Skip).Take(criteria.Take);

                    var ids = query.Select(x => x.Id).ToArray();
                    result.Results = (await GetChannelsByIdsAsync(ids)).AsQueryable().OrderBySortInfos(sortInfos).ToList();
                }
                return result;
            });
        }

        public async Task DeleteChannelsAsync(string[] ids)
        {
            using (var repository = _repositoryFactory())
            {
                await repository.DeleteChannelsAsync(ids);

                await repository.UnitOfWork.CommitAsync();

                CompletenessCacheRegion.ExpireRegion();
            }
        }

        private bool CompareEntries(CompletenessEntry entry, CompletenessEntryEntity entity)
        {
            return entry.ChannelId == entity.ChannelId && entry.ProductId == entity.ProductId;
        }
    }
}
