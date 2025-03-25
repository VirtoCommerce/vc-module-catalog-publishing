using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VirtoCommerce.CatalogPublishingModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;

namespace VirtoCommerce.CatalogPublishingModule.Data.Model
{
    public class CompletenessDetailEntity : Entity, IDataEntity<CompletenessDetailEntity, CompletenessDetail>
    {
        [Required]
        [StringLength(128)]
        public string Name { get; set; }

        [Range(0, 100)]
        public decimal CompletenessPercent { get; set; }

        #region Navigation properties

        public string CompletenessEntryId { get; set; }

        public virtual CompletenessEntryEntity CompletenessEntry { get; set; }

        #endregion

        public virtual CompletenessDetail ToModel(CompletenessDetail detail)
        {
            if (detail == null)
                throw new ArgumentNullException(nameof(detail));

            detail.Name = Name;
            detail.CompletenessPercent = CompletenessPercent;

            return detail;
        }

        public CompletenessDetailEntity FromModel(CompletenessDetail model, PrimaryKeyResolvingMap pkMap)
        {
            ArgumentNullException.ThrowIfNull(model);
            ArgumentNullException.ThrowIfNull(pkMap);

            pkMap.AddPair(model, this);

            Name = model.Name;
            CompletenessPercent = model.CompletenessPercent;

            return this;
        }

        public virtual void Patch(CompletenessDetailEntity detail)
        {
            if (detail == null)
                throw new ArgumentNullException(nameof(detail));

            detail.Name = Name;
            detail.CompletenessPercent = CompletenessPercent;
        }

        public class CompletenessDetailComparer: IEqualityComparer<CompletenessDetailEntity>
        {
            public bool Equals(CompletenessDetailEntity x, CompletenessDetailEntity y)
            {
                return GetHashCode(x) == GetHashCode(y);
            }

            public int GetHashCode(CompletenessDetailEntity obj)
            {
                return obj.Name.GetHashCode();
            }
        }
    }
}
