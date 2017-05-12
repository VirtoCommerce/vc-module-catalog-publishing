﻿using System;
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
    public class ReadinessServiceImpl : ServiceBase, IReadinessService
    {
        private readonly Func<IReadinessRepository> _repositoryFactory;

        public ReadinessServiceImpl(Func<IReadinessRepository> repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }

        public ReadinessChannel[] GetChannelsByIds(string[] ids)
        {
            ReadinessChannel[] retVal = null;
            if (ids != null)
            {
                using (var repository = _repositoryFactory())
                {
                    retVal = repository.GetChannelsByIds(ids).Select(x =>
                    {
                        var channel = x.ToModel(AbstractTypeFactory<ReadinessChannel>.TryCreateInstance());
                        channel.ReadinessPercent = (int)Math.Floor(repository.Entries.Where(e => e.ChannelId == x.Id).Average(e => (int?)e.ReadinessPercent) ?? 0);
                        return channel;
                    }).ToArray();
                }
            }
            return retVal;
        }

        public ReadinessEntry[] GetReadinessEntriesByIds(string[] ids)
        {
            ReadinessEntry[] retVal = null;
            if (ids != null)
            {
                using (var repository = _repositoryFactory())
                {
                    retVal = repository.GetEntriesByIds(ids).Select(x => x.ToModel(AbstractTypeFactory<ReadinessEntry>.TryCreateInstance())).ToArray();
                }
            }
            return retVal;
        }

        public void SaveChannels(ReadinessChannel[] channels)
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
                        var sourceEntity = AbstractTypeFactory<ReadinessChannelEntity>.TryCreateInstance().FromModel(channel, pkMap);
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

        public void SaveEntries(ReadinessEntry[] entries)
        {
            using (var repository = _repositoryFactory())
            {
                using (var changeTracker = GetChangeTracker(repository))
                {
                    var entriesIds = entries.Select(x => x.Id).Where(x => x != null).Distinct().ToArray();
                    var alreadyExistEntities = repository.Entries.Include(x => x.Details).ToArray().Where(x => entriesIds.Contains(x.Id)).ToArray();
                    foreach (var entry in entries)
                    {
                        var sourceEntity = AbstractTypeFactory<ReadinessEntryEntity>.TryCreateInstance().FromModel(entry);
                        var targetEntity = alreadyExistEntities.FirstOrDefault(x => x.Id == entry.Id);
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

        public GenericSearchResult<ReadinessChannel> SearchChannels(ReadinessChannelSearchCriteria criteria)
        {
            var retVal = new GenericSearchResult<ReadinessChannel>();
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
                    sortInfos = new[] { new SortInfo { SortColumn = ReflectionUtility.GetPropertyName<ReadinessChannel>(x => x.Name) } };
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
    }
}