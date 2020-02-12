using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using VirtoCommerce.CatalogPublishingModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogPublishingModule.Data.Model
{
    public class CompletenessChannelEntity : AuditableEntity
    {
        public CompletenessChannelEntity()
        {
            Languages = new NullCollection<CompletenessChannelLanguageEntity>();
            Currencies = new NullCollection<CompletenessChannelCurrencyEntity>();
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

        [Required]
        public string EvaluatorType { get; set; }

        #region Navigation Properties
        
        public virtual ObservableCollection<CompletenessChannelLanguageEntity> Languages { get; set; }
        
        public virtual ObservableCollection<CompletenessChannelCurrencyEntity> Currencies { get; set; }

        #endregion

        public virtual CompletenessChannel ToModel(CompletenessChannel channel)
        {
            if (channel == null)
                throw new ArgumentNullException(nameof(channel));

            channel.Id = Id;

            channel.CreatedBy = CreatedBy;
            channel.CreatedDate = CreatedDate;
            channel.ModifiedBy = ModifiedBy;
            channel.ModifiedDate = ModifiedDate;

            channel.Name = Name;
            channel.CatalogId = CatalogId;
            channel.CatalogName = CatalogName;
            channel.Languages = Languages.Select(x => x.LanguageCode).ToList();
            channel.Currencies = Currencies.Select(x => x.CurrencyCode).ToList();
            channel.EvaluatorType = EvaluatorType;

            return channel;
        }

        public virtual CompletenessChannelEntity FromModel(CompletenessChannel channel, PrimaryKeyResolvingMap pkMap)
        {
            if (channel == null)
                throw new ArgumentNullException(nameof(channel));
            if (pkMap == null)
                throw new ArgumentNullException(nameof(pkMap));

            pkMap.AddPair(channel, this);

            Id = channel.Id;

            CreatedBy = channel.CreatedBy;
            CreatedDate = channel.CreatedDate;
            ModifiedBy = channel.ModifiedBy;
            ModifiedDate = channel.ModifiedDate;

            Name = channel.Name;
            CatalogId = channel.CatalogId;
            CatalogName = channel.CatalogName;
            if (channel.Languages != null)
            {
                Languages = new ObservableCollection<CompletenessChannelLanguageEntity>(channel.Languages.Select(x => new CompletenessChannelLanguageEntity
                {
                    LanguageCode = x
                }));
            }
            if (channel.Currencies != null)
            {
                Currencies = new ObservableCollection<CompletenessChannelCurrencyEntity>(channel.Currencies.Select(x => new CompletenessChannelCurrencyEntity
                {
                    CurrencyCode = x
                }));
            }
            EvaluatorType = channel.EvaluatorType;

            return this;
        }

        public virtual void Patch(CompletenessChannelEntity channel)
        {
            if (channel == null)
                throw new ArgumentNullException(nameof(channel));

            channel.Name = Name;
            channel.CatalogId = CatalogId;
            channel.CatalogName = CatalogName;
            if (!Languages.IsNullCollection())
            {
                var languageComparer = AnonymousComparer.Create((CompletenessChannelLanguageEntity x) => x.LanguageCode);
                Languages.Patch(channel.Languages, languageComparer, (sourceLang, targetLang) => targetLang.LanguageCode = sourceLang.LanguageCode);
            }
            if (!Currencies.IsNullCollection())
            {
                var currencyComparer = AnonymousComparer.Create((CompletenessChannelCurrencyEntity x) => x.CurrencyCode);
                Currencies.Patch(channel.Currencies, currencyComparer, (sourceCurrency, targetCurrency) => targetCurrency.CurrencyCode = sourceCurrency.CurrencyCode);
            }
            channel.EvaluatorType = EvaluatorType;
        }
    }
}