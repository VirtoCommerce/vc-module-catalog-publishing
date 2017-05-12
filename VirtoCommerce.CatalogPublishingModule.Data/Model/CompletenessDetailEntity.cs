using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VirtoCommerce.CatalogPublishingModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogPublishingModule.Data.Model
{
    public class CompletenessDetailEntity : Entity
    {
        [Required]
        [StringLength(128)]
        public string Name { get; set; }
        
        [Range(0, 100)]
        public int CompletenessPercent { get; set; }

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

        public virtual CompletenessDetailEntity FromModel(CompletenessDetail detail)
        {
            if (detail == null)
                throw new ArgumentNullException(nameof(detail));

            Name = detail.Name;
            CompletenessPercent = detail.CompletenessPercent;

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