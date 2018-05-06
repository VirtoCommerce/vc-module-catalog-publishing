using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Unity;
using VirtoCommerce.CatalogPublishingModule.Core.Services;
using VirtoCommerce.CatalogPublishingModule.Data.Model;
using VirtoCommerce.CatalogPublishingModule.Data.Repositories;
using VirtoCommerce.CatalogPublishingModule.Data.Search.Services;
using VirtoCommerce.CatalogPublishingModule.Data.Services;
using VirtoCommerce.CatalogPublishingModule.Data.Services.Evaluation;
using VirtoCommerce.Domain.Search;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.Platform.Data.Infrastructure.Interceptors;
using VirtoCommerce.Platform.Data.Repositories;

namespace VirtoCommerce.CatalogPublishingModule.Web
{
    public class Module : ModuleBase
    {
        private const string _connectionStringName = "VirtoCommerce";
        private readonly IUnityContainer _container;

        public Module(IUnityContainer container)
        {
            _container = container;
        }

        public override void SetupDatabase()
        {
            using (var context = new CompletenessRepositoryImpl(_connectionStringName, _container.Resolve<AuditableInterceptor>()))
            {
                var initializer = new SetupDatabaseInitializer<CompletenessRepositoryImpl, Data.Migrations.Configuration>();
                initializer.InitializeDatabase(context);
            }
        }

        public override void Initialize()
        {
            base.Initialize();

            _container.RegisterType<ICompletenessRepository>(new InjectionFactory(c => new CompletenessRepositoryImpl(_connectionStringName, new EntityPrimaryKeyGeneratorInterceptor(), _container.Resolve<AuditableInterceptor>(),
                new ChangeLogInterceptor(_container.Resolve<Func<IPlatformRepository>>(), ChangeLogPolicy.Cumulative, new[] { typeof(CompletenessEntryEntity).Name }))));
            _container.RegisterType<ICompletenessService, CompletenessServiceImpl>();

            _container.RegisterType<ICompletenessEvaluator, DefaultCompletenessEvaluator>(nameof(DefaultCompletenessEvaluator));
            _container.RegisterType<ICompletenessDetailEvaluator, PropertiesCompletenessDetailEvaluator>(nameof(PropertiesCompletenessDetailEvaluator));
            _container.RegisterType<ICompletenessDetailEvaluator, DescriptionsCompletenessDetailEvaluator>(nameof(DescriptionsCompletenessDetailEvaluator));
            _container.RegisterType<ICompletenessDetailEvaluator, PricesCompletenessDetailEvaluator>(nameof(PricesCompletenessDetailEvaluator));
            _container.RegisterType<ICompletenessDetailEvaluator, SeoCompletenessDetailEvaluator>(nameof(SeoCompletenessDetailEvaluator));
        }

        public override void PostInitialize()
        {
            base.PostInitialize();

            #region Search

            var productIndexingConfigurations = _container.Resolve<IndexDocumentConfiguration[]>();
            if (productIndexingConfigurations != null)
            {
                var productCompletenessDocumentSource = new IndexDocumentSource
                {
                    ChangesProvider = _container.Resolve<ProductCompletenessChangesProvider>(),
                    DocumentBuilder = _container.Resolve<ProductCompletenessDocumentBuilder>(),
                };

                foreach (var configuration in productIndexingConfigurations.Where(c => c.DocumentType == KnownDocumentTypes.Product))
                {
                    if (configuration.RelatedSources == null)
                    {
                        configuration.RelatedSources = new List<IndexDocumentSource>();
                    }

                    configuration.RelatedSources.Add(productCompletenessDocumentSource);
                }
            }

            #endregion
        }
    }
}
