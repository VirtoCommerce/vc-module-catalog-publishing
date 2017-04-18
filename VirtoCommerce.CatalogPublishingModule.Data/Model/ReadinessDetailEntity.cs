using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VirtoCommerce.CatalogPublishingModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogPublishingModule.Data.Model
{
    public class ReadinessDetailEntity : Entity
    {
        [Required]
        [StringLength(128)]
        public string Name { get; set; }
        
        [Range(0, 100)]
        public int ReadinessPercent { get; set; }

        #region Navigation properties

        public string ReadinessEntryId { get; set; }

        public virtual ReadinessEntryEntity ReadinessEntry { get; set; }

        #endregion

        public virtual ReadinessDetail ToModel(ReadinessDetail detail)
        {
            if (detail == null)
                throw new ArgumentNullException("detail");
            
            detail.Name = Name;
            detail.ReadinessPercent = ReadinessPercent;

            return detail;
        }

        public virtual ReadinessDetailEntity FromModel(ReadinessDetail detail)
        {
            if (detail == null)
                throw new ArgumentNullException("detail");

            Name = detail.Name;
            ReadinessPercent = detail.ReadinessPercent;

            return this;
        }

        public virtual void Patch(ReadinessDetailEntity detail)
        {
            if (detail == null)
                throw new ArgumentNullException("detail");

            detail.Name = Name;
            detail.ReadinessPercent = ReadinessPercent;
        }

        public class ReadinessDetailComparer: IEqualityComparer<ReadinessDetailEntity>
        {
            public bool Equals(ReadinessDetailEntity x, ReadinessDetailEntity y)
            {
                return GetHashCode(x) == GetHashCode(y);
            }

            public int GetHashCode(ReadinessDetailEntity obj)
            {
                var result = string.Join(":", obj.Name, obj.ReadinessPercent);
                return result.GetHashCode();
            }
        }
    }
}