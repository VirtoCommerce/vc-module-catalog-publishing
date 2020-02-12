using System;
using System.Data.Entity;
using System.Linq;
using VirtoCommerce.CatalogPublishingModule.Core.Model;
using VirtoCommerce.CatalogPublishingModule.Core.Model.Search;
using VirtoCommerce.CatalogPublishingModule.Core.Services;
using VirtoCommerce.CatalogPublishingModule.Data.Model;
using VirtoCommerce.CatalogPublishingModule.Data.Repositories;
using VirtoCommerce.Domain.Commerce.Model.Search;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.CatalogPublishingModule.Data.Services
{
    public class CompletenessServiceImpl : ServiceBase, ICompletenessService
    {
        private readonly Func<ICompletenessRepository> _repositoryFactory;

        public CompletenessServiceImpl(Func<ICompletenessRepository> repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }

        public CompletenessChannel[] GetChannelsByIds(string[] ids)
        {
            CompletenessChannel[] retVal = null;
            if (ids != null)
            {
                using (var repository = _repositoryFactory())
                {
                    retVal = repository.GetChannelsByIds(ids).Select(x =>
                    {
                        var channel = x.ToModel(AbstractTypeFactory<CompletenessChannel>.TryCreateInstance());
                        channel.CompletenessPercent = (int)Math.Floor(repository.Entries.Where(e => e.ChannelId == x.Id).Average(e => (int?)e.CompletenessPercent) ?? 0);
                        return channel;
                    }).ToArray();
                }
            }
            return retVal;
        }

        public CompletenessEntry[] GetCompletenessEntriesByIds(string[] ids)
        {
            CompletenessEntry[] retVal = null;
            if (ids != null)
            {
                using (var repository = _repositoryFactory())
                {
                    retVal = repository.GetEntriesByIds(ids).Select(x => x.ToModel(AbstractTypeFactory<CompletenessEntry>.TryCreateInstance())).ToArray();
                }
            }
            return retVal;
        }

        public void SaveChannels(CompletenessChannel[] channels)
        {
            var pkMap = new PrimaryKeyResolvingMap();
            using (var repository = _repositoryFactory())
            {
                using (var changeTracker = GetChangeTracker(repository))
                {
                    var ids = channels.Select(x => x.Id).Where(x => x != null).Distinct().ToArray();
                    var alreadyExistEntities = repository.Channels.Where(x => ids.Contains(x.Id)).ToArray();
                    foreach (var channel in channels)
                    {
                        var sourceEntity = AbstractTypeFactory<CompletenessChannelEntity>.TryCreateInstance().FromModel(channel, pkMap);
                        var targetEntity = alreadyExistEntities.FirstOrDefault(x => x.Id == channel.Id);
                        if (targetEntity != null)
                        {
                            changeTracker.Attach(targetEntity);
                            sourceEntity.Patch(targetEntity);
                        }
                        else
                        {
                            repository.Add(sourceEntity);
                        }
                    }
                }
                CommitChanges(repository);
                pkMap.ResolvePrimaryKeys();
            }
        }

        public void SaveEntries(CompletenessEntry[] entries)
        {
            using (var repository = _repositoryFactory())
            {
                using (var changeTracker = GetChangeTracker(repository))
                {
                    var alreadyExistEntities = repository.Entries.Include(x => x.Details).ToArray().Where(x => entries.Any(y => CompareEntries(y, x))).ToArray();
                    foreach (var entry in entries)
                    {
                        var sourceEntity = AbstractTypeFactory<CompletenessEntryEntity>.TryCreateInstance().FromModel(entry);
                        var targetEntity = alreadyExistEntities.FirstOrDefault(x => CompareEntries(entry, x));
                        if (targetEntity != null)
                        {
                            changeTracker.Attach(targetEntity);
                            sourceEntity.Patch(targetEntity);
                        }
                        else
                        {
                            repository.Add(sourceEntity);
                        }
                    }
                }
                CommitChanges(repository);
            }
        }

        public GenericSearchResult<CompletenessChannel> SearchChannels(CompletenessChannelSearchCriteria criteria)
        {
            var retVal = new GenericSearchResult<CompletenessChannel>();
            using (var repository = _repositoryFactory())
            {
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

                retVal.TotalCount = query.Count();
                query = query.Skip(criteria.Skip).Take(criteria.Take);

                var ids = query.Select(x => x.Id).ToArray();
                retVal.Results = GetChannelsByIds(ids).AsQueryable().OrderBySortInfos(sortInfos).ToList();
            }
            return retVal;
        }

        public void DeleteChannels(string[] ids)
        {
            using (var repository = _repositoryFactory())
            {
                repository.DeleteChannels(ids);
                CommitChanges(repository);
            }
        }

        private bool CompareEntries(CompletenessEntry entry, CompletenessEntryEntity entity)
        {
            return entry.ChannelId == entity.ChannelId && entry.ProductId == entity.ProductId;
        }
    }
}