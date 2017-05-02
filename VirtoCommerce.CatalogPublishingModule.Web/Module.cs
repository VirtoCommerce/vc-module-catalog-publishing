using Microsoft.Practices.Unity;
using VirtoCommerce.CatalogPublishingModule.Core.Services;
using VirtoCommerce.CatalogPublishingModule.Data.Repositories;
using VirtoCommerce.CatalogPublishingModule.Data.Services;
using VirtoCommerce.CatalogPublishingModule.Data.Services.Evaluators;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.Platform.Data.Infrastructure.Interceptors;
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
            using (var context = new ReadinessRepositoryImpl(ConnectionStringName, _container.Resolve<AuditableInterceptor>()))
            {
                var initializer = new SetupDatabaseInitializer<ReadinessRepositoryImpl, Data.Migrations.Configuration>();
                initializer.InitializeDatabase(context);
            }
        }

        public override void Initialize()
        {
            base.Initialize();

            _container.RegisterType<IReadinessRepository>(new InjectionFactory(c => new ReadinessRepositoryImpl(ConnectionStringName, new EntityPrimaryKeyGeneratorInterceptor(), _container.Resolve<AuditableInterceptor>())));
            _container.RegisterType<IReadinessService, ReadinessServiceImpl>();
            _container.RegisterType<IBatchDocumentBuilder<CatalogProduct>, ProductReadinessDocumentBuilder>(nameof(ProductReadinessDocumentBuilder));
            _container.RegisterType<IReadinessEvaluator, DefaultReadinessEvaluator>(nameof(DefaultReadinessEvaluator));
            _container.RegisterType<DefaultReadinessDetailEvaluator, PropertiesReadinessDetailEvaluator>(nameof(PropertiesReadinessDetailEvaluator));
            _container.RegisterType<DefaultReadinessDetailEvaluator, DescriptionsReadinessDetailEvaluator>(nameof(DescriptionsReadinessDetailEvaluator));
            _container.RegisterType<DefaultReadinessDetailEvaluator, PricesReadinessDetailEvaluator>(nameof(PricesReadinessDetailEvaluator));
            _container.RegisterType<DefaultReadinessDetailEvaluator, SeoReadinessDetailEvaluator>(nameof(SeoReadinessDetailEvaluator));
        }
    }
}