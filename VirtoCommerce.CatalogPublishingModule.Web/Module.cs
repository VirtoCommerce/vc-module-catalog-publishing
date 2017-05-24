using System;
using Microsoft.Practices.Unity;
using VirtoCommerce.CatalogPublishingModule.Core.Services;
using VirtoCommerce.CatalogPublishingModule.Data.Model;
using VirtoCommerce.CatalogPublishingModule.Data.Repositories;
using VirtoCommerce.CatalogPublishingModule.Data.Search.Services;
using VirtoCommerce.CatalogPublishingModule.Data.Services;
using VirtoCommerce.CatalogPublishingModule.Data.Services.Evaluation;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.Platform.Data.Infrastructure.Interceptors;
using VirtoCommerce.Platform.Data.Repositories;
using VirtoCommerce.SearchModule.Core.Model.Indexing;

namespace VirtoCommerce.CatalogPublishingModule.Web
{
    public class Module : ModuleBase
    {
        private const string ConnectionStringName = "VirtoCommerce";
        private readonly IUnityContainer _container;

        public Module(IUnityContainer container)
        {
            _container = container;
        }

        public override void SetupDatabase()
        {
            using (var context = new CompletenessRepositoryImpl(ConnectionStringName, _container.Resolve<AuditableInterceptor>()))
            {
                var initializer = new SetupDatabaseInitializer<CompletenessRepositoryImpl, Data.Migrations.Configuration>();
                initializer.InitializeDatabase(context);
            }
        }

        public override void Initialize()
        {
            base.Initialize();

            _container.RegisterType<ICompletenessRepository>(new InjectionFactory(c => new CompletenessRepositoryImpl(ConnectionStringName, new EntityPrimaryKeyGeneratorInterceptor(), _container.Resolve<AuditableInterceptor>(),
                new ChangeLogInterceptor(_container.Resolve<Func<IPlatformRepository>>(), ChangeLogPolicy.Cumulative, new[] { typeof(CompletenessEntryEntity).Name }, _container.Resolve<IUserNameResolver>()))));
            _container.RegisterType<ICompletenessService, CompletenessServiceImpl>();

            _container.RegisterType<ICompletenessEvaluator, DefaultCompletenessEvaluator>(nameof(DefaultCompletenessEvaluator));
            _container.RegisterType<ICompletenessDetailEvaluator, PropertiesCompletenessDetailEvaluator>(nameof(PropertiesCompletenessDetailEvaluator));
            _container.RegisterType<ICompletenessDetailEvaluator, DescriptionsCompletenessDetailEvaluator>(nameof(DescriptionsCompletenessDetailEvaluator));
            _container.RegisterType<ICompletenessDetailEvaluator, PricesCompletenessDetailEvaluator>(nameof(PricesCompletenessDetailEvaluator));
            _container.RegisterType<ICompletenessDetailEvaluator, SeoCompletenessDetailEvaluator>(nameof(SeoCompletenessDetailEvaluator));

            _container.RegisterType<IOperationProvider, ProductCompletenessOperationProvider>(nameof(ProductCompletenessOperationProvider));
            _container.RegisterType<IBatchDocumentBuilder<CatalogProduct>, ProductCompletenessDocumentBuilder>(nameof(ProductCompletenessDocumentBuilder));
        }
    }
}