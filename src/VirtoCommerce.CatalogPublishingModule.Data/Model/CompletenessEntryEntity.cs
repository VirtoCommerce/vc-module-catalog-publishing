using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using VirtoCommerce.CatalogPublishingModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;

namespace VirtoCommerce.CatalogPublishingModule.Data.Model
{
    public class CompletenessEntryEntity : AuditableEntity, IDataEntity<CompletenessEntryEntity, CompletenessEntry>
    {
        public CompletenessEntryEntity()
        {
            Details = new NullCollection<CompletenessDetailEntity>();
        }

        [Required]
        [StringLength(128)]
        public string ProductId { get; set; }

        [Range(0, 100)]
        public decimal CompletenessPercent { get; set; }

        #region Navigation properties

        public virtual ObservableCollection<CompletenessDetailEntity> Details { get; set; }

        public string ChannelId { get; set; }

        public virtual CompletenessChannelEntity Channel { get; set; }

        #endregion

        public virtual CompletenessEntry ToModel(CompletenessEntry entry)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            entry.Id = Id;

            entry.CreatedBy = CreatedBy;
            entry.CreatedDate = CreatedDate;
            entry.ModifiedBy = ModifiedBy;
            entry.ModifiedDate = ModifiedDate;

            entry.ChannelId = ChannelId;
            entry.ProductId = ProductId;
            entry.CompletenessPercent = CompletenessPercent;
            entry.Details = Details.Select(x =>
            {
                var detail = x.ToModel(AbstractTypeFactory<CompletenessDetail>.TryCreateInstance());
                detail.ProductId = ProductId;
                return detail;
            }).ToArray();

            return entry;
        }

        public CompletenessEntryEntity FromModel(CompletenessEntry model, PrimaryKeyResolvingMap pkMap)
        {
            ArgumentNullException.ThrowIfNull(model);
            ArgumentNullException.ThrowIfNull(pkMap);

            pkMap.AddPair(model, this);

            Id = model.Id;

            CreatedBy = model.CreatedBy;
            CreatedDate = model.CreatedDate;
            ModifiedBy = model.ModifiedBy;
            ModifiedDate = model.ModifiedDate;

            ChannelId = model.ChannelId;
            ProductId = model.ProductId;
            CompletenessPercent = model.CompletenessPercent;
            if (model.Details != null)
            {
                Details = new ObservableCollection<CompletenessDetailEntity>(model.Details.Select(x => AbstractTypeFactory<CompletenessDetailEntity>.TryCreateInstance().FromModel(x, pkMap)));
            }

            return this;
        }

        public virtual void Patch(CompletenessEntryEntity entry)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            entry.ChannelId = ChannelId;
            entry.ProductId = ProductId;
            entry.CompletenessPercent = CompletenessPercent;

            if (!Details.IsNullCollection())
            {
                Details.Patch(entry.Details, new CompletenessDetailEntity.CompletenessDetailComparer(), (sourceItem, targetItem) => sourceItem.Patch(targetItem));
            }
        }
    }
}
