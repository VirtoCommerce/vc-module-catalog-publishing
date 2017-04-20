using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using VirtoCommerce.CatalogPublishingModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogPublishingModule.Data.Model
{
    public class ReadinessEntryEntity : Entity
    {
        public ReadinessEntryEntity()
        {
            Details = new NullCollection<ReadinessDetailEntity>();
        }

        [Required]
        [StringLength(128)]
        public string ProductId { get; set; }
        
        [Range(0, 100)]
        public int ReadinessPercent { get; set; }

        #region Navigation properties

        public virtual ObservableCollection<ReadinessDetailEntity> Details { get; set; }
        
        public string ChannelId { get; set; }

        public virtual ReadinessChannelEntity Channel { get; set; }

        #endregion

        public virtual ReadinessEntry ToModel(ReadinessEntry entry)
        {
            if (entry == null)
                throw new ArgumentNullException("entry");

            entry.ChannelId = ChannelId;
            entry.ProductId = ProductId;
            entry.ReadinessPercent = ReadinessPercent;
            entry.Details = Details.Select(x => x.ToModel(AbstractTypeFactory<ReadinessDetail>.TryCreateInstance())).ToArray();

            return entry;
        }

        public virtual ReadinessEntryEntity FromModel(ReadinessEntry entry)
        {
            if (entry == null)
                throw new ArgumentNullException("entry");

            ChannelId = entry.ChannelId;
            ProductId = entry.ProductId;
            ReadinessPercent = entry.ReadinessPercent;
            if (entry.Details != null)
            {
                Details = new ObservableCollection<ReadinessDetailEntity>(entry.Details.Select(x => AbstractTypeFactory<ReadinessDetailEntity>.TryCreateInstance().FromModel(x)));
            }

            return this;
        }

        public virtual void Patch(ReadinessEntryEntity entry)
        {
            if (entry == null)
                throw new ArgumentNullException("entry");

            entry.ChannelId = ChannelId;
            entry.ProductId = ProductId;
            entry.ReadinessPercent = ReadinessPercent;

            if (!Details.IsNullCollection())
            {
                Details.Patch(entry.Details, new ReadinessDetailEntity.ReadinessDetailComparer(), (sourceItem, targetItem) => sourceItem.Patch(targetItem));
            }
        }
    }
}