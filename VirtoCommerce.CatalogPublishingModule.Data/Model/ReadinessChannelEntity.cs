﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using VirtoCommerce.CatalogPublishingModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogPublishingModule.Data.Model
{
    public class ReadinessChannelEntity : AuditableEntity
    {
        public ReadinessChannelEntity()
        {
            Entries = new NullCollection<ReadinessEntryEntity>();
        }

        [Required]
        [StringLength(128)]
        public string Name { get; set; }
        
        [Required]
        [StringLength(128)]
        public string CatalogId { get; set; }

        [Required]
        [StringLength(128)]
        public string CatalogName { get; set; }

        public string Language { get; set; }

        [StringLength(128)]
        public string PricelistId { get; set; }

        public string EvaluatorType { get; set; }

        #region Navigation properties

        public virtual ObservableCollection<ReadinessEntryEntity> Entries { get; set; }

        #endregion

        public virtual ReadinessChannel ToModel(ReadinessChannel channel)
        {
            if (channel == null)
                throw new ArgumentNullException("channel");

            channel.Id = Id;

            channel.Name = Name;
            channel.CatalogId = CatalogId;
            channel.CatalogName = CatalogName;
            channel.Language = Language;
            channel.PricelistId = PricelistId;
            channel.EvaluatorType = EvaluatorType;

            channel.CreatedBy = CreatedBy;
            channel.CreatedDate = CreatedDate;
            channel.ModifiedBy = ModifiedBy;
            channel.ModifiedDate = ModifiedDate;

            return channel;
        }

        public virtual ReadinessChannelEntity FromModel(ReadinessChannel channel, PrimaryKeyResolvingMap pkMap)
        {
            if (channel == null)
                throw new ArgumentNullException("channel");
            if (pkMap == null)
                throw new ArgumentNullException("pkMap");

            pkMap.AddPair(channel, this);

            Id = channel.Id;

            Name = channel.Name;
            CatalogId = channel.CatalogId;
            CatalogName = channel.CatalogName;
            Language = channel.Language;
            PricelistId = channel.PricelistId;
            EvaluatorType = channel.EvaluatorType;

            CreatedBy = channel.CreatedBy;
            CreatedDate = channel.CreatedDate;
            ModifiedBy = channel.ModifiedBy;
            ModifiedDate = channel.ModifiedDate;

            return this;
        }

        public virtual void Patch(ReadinessChannelEntity channel)
        {
            if (channel == null)
                throw new ArgumentNullException("channel");

            channel.Name = Name;
            channel.CatalogId = CatalogId;
            channel.CatalogName = CatalogName;
            channel.Language = Language;
            channel.PricelistId = PricelistId;
            channel.EvaluatorType = EvaluatorType;
        }
    }
}